using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Marigergis.Attendance.WebApi.Models.Features.Hubs;

[Authorize]
public class BroadcastHub : Hub
{
}
