using Bogus;
using Marigergis.Attendance.WebApi.Data;
using Marigergis.Attendance.WebApi.Models.Constants;
using Marigergis.Attendance.WebApi.Models.Entities;
using Marigergis.Attendance.WebApi.Models.Features.Accounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Marigergis.Attendance.WebApi.Data;

public static class SeedData
{
    /// <summary>
    /// Database migration
    /// </summary>
    /// <param name="services"></param>
    public static void InitializeDatabase(IServiceProvider services)
    {
        using (IServiceScope serviceScope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
            {
                if (!context!.Database.ProviderName!.Contains("InMemory"))
                {
                    context.Database.Migrate();
                }
            }
        }
    }

    /// <summary>
    /// Create default admin user and application roles
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static async Task CreateDefaultAdminAndRoles(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<CustomUser>>();
        var roles = new Roles();

        // Add every role from roles if it doesn't exists
        foreach (var role in roles.ArrayList)
        {
            var isRoleExists = await roleManager.RoleExistsAsync(role);
            if (!isRoleExists)
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role {role}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        // Create default admin if it doesn't exists
        var isDefaultAdminExists = await userManager.FindByNameAsync(DefaultAdmin.UserName);
        if (isDefaultAdminExists == null)
        {
            // Get the username/password from configuration file
            var defaultAdmin = new CustomUser { UserName = DefaultAdmin.UserName };
            var createResult = await userManager.CreateAsync(defaultAdmin, DefaultAdmin.Password);

            if (!createResult.Succeeded)
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            // Add 'Admin' role
            var roleResult = await userManager.AddToRoleAsync(defaultAdmin, roles.Admin);
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to add admin to role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
    }

    /// <summary>
    /// Set up default attendance configuration
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static async Task AttendanceConfiguration(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Check if configuration already exists
        if (!context.Config!.Any())
        {
            // Add default configurations from config file
            await context.Config!.AddAsync(new Config
            {
                TimeIn = AttendanceConfig.TimeIn,
                TimeOut = AttendanceConfig.TimeOut,
                GracePeriod = AttendanceConfig.GracePeriod
            });

            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seed fake employees with logs
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static async Task EnsureSeedEmployeesAndLogs(IServiceProvider services)
    {
        using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
            {
                const int NUMBER_OF_EMPLOYEE = 12;
                const int NUMBER_OF_LOGS_PER_EMPLOYEE = 7;
                const string DEFAULT_PASSWORD = "P@$$w0rd";

                // Check if Employee table is empty
                if (!context.Employees!.Any())
                {
                    // Set fake data for employee
                    var fakeEmployee = new Faker<RegisterViewModel>()
                        .RuleFor(m => m.UserName, f => f.Internet.UserName())
                        .RuleFor(m => m.Password, f => DEFAULT_PASSWORD)
                        .RuleFor(m => m.FullName, f => f.Name.FirstName() + " " + f.Name.LastName())
                        .RuleFor(m => m.CardNo, f => f.Random.Number(10000, 99999).ToString())
                        .RuleFor(m => m.Position, f => f.PickRandom(new[] { "Manager", "Developer", "Designer", "Analyst", "HR Specialist", "Accountant" }));

                    // Set up for generating fake logs
                    var gracePeriod = Convert.ToInt32(AttendanceConfig.GracePeriod) * 2;
                    var timeInString = $"{DateTime.Today.ToString("d")} {AttendanceConfig.TimeIn}";
                    var timeOutString = $"{DateTime.Today.ToString("d")} {AttendanceConfig.TimeOut}";
                    var timeIn = DateTime.Parse(timeInString).ToUniversalTime().AddDays(-1);
                    var timeOut = DateTime.Parse(timeOutString).ToUniversalTime().AddDays(-1);

                    var timeInRange = new DateTime[] { timeIn.AddMinutes(-gracePeriod), timeIn.AddMinutes(gracePeriod) };
                    var timeOutRange = new DateTime[] { timeOut.AddMinutes(-gracePeriod), timeOut.AddMinutes(gracePeriod) };

                    // Set fake data for logs
                    var fakeLog = new Faker<Log>()
                        .RuleFor(m => m.TimeIn, f => f.Date.Between(timeInRange[0], timeInRange[1]))
                        .RuleFor(m => m.TimeOut, f => f.Date.Between(timeOutRange[0], timeOutRange[1]));

                    // Generate List of fake employees
                    var fakeEmployeeList = fakeEmployee.Generate(NUMBER_OF_EMPLOYEE);

                    // Iterate each fake list
                    foreach (var employee in fakeEmployeeList)
                    {
                        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                        var userManager = services.GetRequiredService<UserManager<CustomUser>>();
                        var daysToMinus = 0;

                        // Generate List of fake logs
                        var fakeLogList = fakeLog.Generate(NUMBER_OF_LOGS_PER_EMPLOYEE);
                        fakeLogList = fakeLogList.Select(m =>
                        {
                            m.TimeIn = Convert.ToDateTime(m.TimeIn).AddDays(daysToMinus);
                            m.TimeOut = Convert.ToDateTime(m.TimeOut).AddDays(daysToMinus);
                            m.Created = Convert.ToDateTime(m.TimeIn);
                            daysToMinus--;
                            return m;
                        }).ToList();

                        // Create Employee & Logs, generated by fake data
                        var employeeInfo = new Employee
                        {
                            FullName = employee.FullName,
                            CardNo = employee.CardNo,
                            Position = employee.Position,
                            Status = Status.Active,
                            Logs = fakeLogList.ToList()
                        };
                        var roles = new Roles();
                        var user = new CustomUser { UserName = employee.UserName, Employee = employeeInfo };
                        await userManager.CreateAsync(user, DEFAULT_PASSWORD);
                        await userManager.AddToRoleAsync(user, roles.Employee);
                    }
                }
            }
        }
    }
}
