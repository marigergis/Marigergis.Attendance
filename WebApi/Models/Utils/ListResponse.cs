using Marigergis.Attendance.WebApi.Models.Entities;

namespace Marigergis.Attendance.WebApi.Models.Features.Utils;

public class ListResponse<T> where T : class
{
    public DateFilteredList? Meta { get; set; }
    public ICollection<T>? Data { get; set; }
}
