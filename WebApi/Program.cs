using Marigergis.Attendance.WebApi.Data;
using Marigergis.Attendance.WebApi.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure identity database access via EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

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

app.MapIdentityApi<CustomUser>();

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
