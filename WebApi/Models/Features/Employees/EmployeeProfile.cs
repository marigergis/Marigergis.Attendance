using AutoMapper;

namespace Marigergis.Attendance.WebApi.Models.Features.Employees;

    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<Entities.Employee, EmployeeViewModel>();
            CreateMap<EmployeeViewModel, Entities.Employee>();
        }
    }
