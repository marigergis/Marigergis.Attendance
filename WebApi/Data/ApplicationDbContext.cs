using System;
using Marigergis.Attendance.WebApi.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Marigergis.Attendance.WebApi.Data;

public class ApplicationDbContext : IdentityDbContext<CustomUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    { }

    public DbSet<Employee>? Employees { get; set; }
    public DbSet<Log>? Logs { get; set; }
    public DbSet<Config>? Config { get; set; }
}
