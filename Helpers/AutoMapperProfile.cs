using AutoMapper;
using HRMSystem.DTOs;
using HRMSystem.Models;

namespace HRMSystem.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<EmployeeType, EmployeeTypeDto>().ReverseMap();
            CreateMap<Department, DepartmentDto>().ReverseMap();
            CreateMap<EmployeeCreateDto, Employee>()
                .ForMember(dest => dest.DepartmentId,
                    opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest => dest.EmployeeTypeId,
                    opt => opt.MapFrom(src => src.EmployeeTypeId))
                .ForMember(dest => dest.Status,
                    opt => opt.Ignore());

            CreateMap<EmployeeUpdateDto, Employee>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.HashPassword, opt => opt.Ignore());

            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DepartmentName,
                    opt => opt.MapFrom(src => src.Departments != null ? src.Departments.DepartmentName : null))
                .ForMember(dest => dest.TypeName,
                    opt => opt.MapFrom(src => src.EmployeeTypes != null ? src.EmployeeTypes.TypeName : null))
                .ForMember(dest => dest.RoleName,
                    opt => opt.MapFrom(src => src.EmployeeRoles.FirstOrDefault() != null
                        ? src.EmployeeRoles.FirstOrDefault().Roles.RoleName
                        : null))
                .ForMember(dest => dest.RoleId,
                    opt => opt.MapFrom(src => src.EmployeeRoles.FirstOrDefault() != null
                        ? src.EmployeeRoles.FirstOrDefault().RoleId
                        : 0));

            CreateMap<RecruitmentPosition, RecruitmentPositionDto>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Departments.DepartmentName));
            CreateMap<RecruitmentPositionDto, RecruitmentPosition>()
                .ForMember(dest => dest.Departments, opt => opt.Ignore());

            CreateMap<RecruitmentRequirementDto, RecruitmentRequirement>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (RecruitmentStatus)src.Status))
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));
            CreateMap<RecruitmentRequirement, RecruitmentRequirementDto>()
                 .ForMember(dest => dest.PositionName, opt => opt.MapFrom(src => src.RecruitmentPositions.PositionName))
                 .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employees.FullName));


            CreateMap<OvertimeRequest, OvertimeRequestDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employees != null ? src.Employees.FullName : null))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedBy != null ? src.ApprovedBy.FullName : null))
                .ReverseMap()
                .ForMember(dest => dest.Employees, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore());

            CreateMap<LeaveRequest, LeaveRequestDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employees != null ? src.Employees.FullName : null))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedBy != null ? src.ApprovedBy.FullName : null))
                .ReverseMap()
                .ForMember(dest => dest.Employees, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore());

            CreateMap<Attendance, AttendanceDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employees.FullName));

            CreateMap<JobPost, JobPostDto>()
                .ForMember(dest => dest.PostedBy,
                           opt => opt.MapFrom(src => src.PostedBy.FullName))
                .ForMember(dest => dest.Requirement,
                           opt => opt.MapFrom(src => src.RecruitmentRequirements.Requirement))
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => (int)src.Status))
                .ReverseMap()
                .ForMember(dest => dest.PostedBy, opt => opt.Ignore())
                .ForMember(dest => dest.RecruitmentRequirements, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (JobPostStatus)src.Status));

        }
    }
}
