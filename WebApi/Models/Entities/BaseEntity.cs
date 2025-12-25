using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marigergis.Attendance.WebApi.Models.Entities;

public abstract class BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public DateTime? Updated { get; set; }

    public DateTime? Deleted { get; set; }
}
