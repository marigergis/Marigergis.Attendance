using Marigergis.Attendance.WebApi.Data;
using Marigergis.Attendance.WebApi.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure identity database access via EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// MediatR for CQRS handlers
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// AutoMapper profiles
builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);

// Authorization
builder.Services.AddAuthorization();

// Activate identity APIs. By default, both cookies and proprietary tokens
// are activated. Cookies will be issued based on the 'useCookies' querystring
// parameter in the login endpoint
builder.Services.AddIdentityApiEndpoints<CustomUser>(options =>
{
    // Configure password requirements (relaxed for development)
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "My WebAPI");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Custom login endpoint that accepts username instead of email
app.MapPost("/login", async (
    string? useCookies,
    string? useSessionCookies,
    LoginRequest loginRequest,
    SignInManager<CustomUser> signInManager,
    UserManager<CustomUser> userManager) =>
{
    useCookies ??= "false";
    useSessionCookies ??= "false";

    if (!bool.TryParse(useCookies, out var useCookieScheme) || !bool.TryParse(useSessionCookies, out var useSessionScheme))
    {
        return Results.BadRequest();
    }

    if (useSessionScheme && useCookieScheme)
    {
        return Results.BadRequest("useCookies and useSessionCookies are mutually exclusive");
    }

    // Find user by username instead of email
    var user = await userManager.FindByNameAsync(loginRequest.Username);
    if (user == null)
    {
        return Results.Unauthorized();
    }

    var result = await signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, lockoutOnFailure: true);
    if (!result.Succeeded)
    {
        return Results.Unauthorized();
    }

    if (useSessionScheme)
    {
        await signInManager.SignInAsync(user, new AuthenticationProperties { IsPersistent = false }, "Identity.Application");
    }
    else if (useCookieScheme)
    {
        await signInManager.SignInAsync(user, new AuthenticationProperties { IsPersistent = true }, "Identity.Application");
    }
    else
    {
        return Results.Unauthorized();
    }

    return Results.Ok();
})
.WithName("Login")
.WithOpenApi()
.AllowAnonymous();

// Map other identity endpoints (register, refresh, logout, etc.)
app.MapPost("/register", async (
    RegisterRequest registration,
    UserManager<CustomUser> userManager) =>
{
    var user = new CustomUser { UserName = registration.Email, Email = registration.Email };

    var result = await userManager.CreateAsync(user, registration.Password);

    if (!result.Succeeded)
    {
        return Results.BadRequest(result.Errors);
    }

    return Results.StatusCode(StatusCodes.Status201Created);
})
.WithName("Register")
.WithOpenApi()
.AllowAnonymous();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();



    if (!app.Environment.IsEnvironment("Test"))
    {

        SeedData.InitializeDatabase(services);
        SeedData.CreateDefaultAdminAndRoles(services).Wait();
        SeedData.AttendanceConfiguration(services).Wait();

        if (!app.Environment.IsProduction())
        {
            SeedData.EnsureSeedEmployeesAndLogs(services).Wait();
        }
    }
}

app.Run();
