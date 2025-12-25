using System.ComponentModel.DataAnnotations;
using Marigergis.Attendance.WebApi.Models.Entities;
using Marigergis.Attendance.WebApi.Models.Features.Accounts;

namespace Marigergis.Attendance.WebApi.Models.Features.Employees;

public class EmployeeViewModel : BaseEntity
{
    [Required]
    public string? IdentityId { get; set; }
    [Required]
    public string? FullName { get; set; }
    public string? Position { get; set; }
    [Required]
    public string? CardNo { get; set; }
    public Status Status { get; set; }

    public virtual UserViewModel? Identity { get; set; }
}
