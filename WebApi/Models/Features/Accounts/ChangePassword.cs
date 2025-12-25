using System.Security.Claims;
using Marigergis.Attendance.WebApi.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Marigergis.Attendance.WebApi.Models.Features.Accounts;

public class ChangePassword
{
    public class Command : IRequest<IdentityResult>
    {
        public Command(ChangePasswordViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public ChangePasswordViewModel ViewModel { get; }
    }

    public class CommandHandler : IRequestHandler<Command, IdentityResult>
    {
        private readonly UserManager<CustomUser> _manager;
        private readonly IHttpContextAccessor _httpContext;

        public CommandHandler(UserManager<CustomUser> manager, IHttpContextAccessor httpContext)
        {
            _manager = manager;
            _httpContext = httpContext;
        }

        public async Task<IdentityResult> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                // Get the username in sub type claim
                var username = _httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get account details
                var user = await _manager.FindByNameAsync(username!);

                // Change account password then return result
                return await _manager.ChangePasswordAsync(
                    user!,
                    request.ViewModel.OldPassword!,
                    request.ViewModel.NewPassword!
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
