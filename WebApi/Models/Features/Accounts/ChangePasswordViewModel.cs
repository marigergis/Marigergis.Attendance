using System.ComponentModel.DataAnnotations;

namespace Marigergis.Attendance.WebApi.Models.Features.Accounts;

public class ChangePasswordViewModel
{
    [Required]
    public string? OldPassword { get; set; }
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    public string? NewPassword { get; set; }
}
