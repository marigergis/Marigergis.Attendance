namespace Marigergis.Attendance.WebApi.Models.Entities;

public class DateFilteredList : BasePagedList
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
