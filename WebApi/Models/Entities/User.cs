using Microsoft.AspNetCore.Identity;

namespace Marigergis.Attendance.WebApi.Models.Entities;

public class CustomUser : IdentityUser
{
    public virtual Employee? Employee { get; set; }
}
