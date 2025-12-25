using AutoMapper;
using Marigergis.Attendance.WebApi.Models.Features.Config;

namespace Marigergis.Attendance.WebApi.Models.Features.Config;

public class ConfigProfile : Profile
{
    public ConfigProfile()
    {
        CreateMap<Entities.Config, ConfigViewModel>();
        CreateMap<ConfigViewModel, Entities.Config>();
    }
}
