using AutoMapper;
using Marigergis.Attendance.WebApi.Models.Entities;

namespace Marigergis.Attendance.WebApi.Models.Features.Accounts;

public class AccountProfile : Profile
{
    public AccountProfile()
    {
        CreateMap<UserViewModel, CustomUser>();
        CreateMap<CustomUser, UserViewModel>();
    }
}
