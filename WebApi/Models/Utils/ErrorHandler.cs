using System.Text.Json.Serialization;

namespace Marigergis.Attendance.WebApi.Models.Features.Utils;

public class ErrorHandler
{
    [JsonPropertyName("error_description")]
    public string? Description { get; set; }
}
