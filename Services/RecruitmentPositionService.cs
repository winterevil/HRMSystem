using System.Security.Claims;
using AutoMapper;
using HRMSystem.DTOs;
using HRMSystem.Models;
using HRMSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Services
{
    public class RecruitmentPositionService : IRecruitmentPositionService
    {
        private readonly IRecruimentPositionRepository _repo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        public RecruitmentPositionService(IRecruimentPositionRepository repo, IMapper mapper, IEmployeeRepository employeeRepository, INotificationService notificationService)
        {
            _repo = repo;
            _mapper = mapper;
            _employeeRepo = employeeRepository;
            _notificationService = notificationService;
        }
        public async Task CreateAsync(RecruitmentPositionDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to create recruitment positions.");
            }

            var entity = _mapper.Map<RecruitmentPosition>(dto);

            if (entity == null)
            {
                throw new InvalidOperationException("Failed to create recruitment position.");
            }

            entity.CreatedAt = DateTime.UtcNow.AddHours(7);
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
            var recipients = new List<int>();

            var hrUsers = await _employeeRepo.GetEmployeesByRoleAsync("HR");
            recipients.AddRange(hrUsers.Select(h => h.Id));

            var managers = await _employeeRepo.GetManagersByDepartmentAsync(entity.DepartmentId);
            recipients.AddRange(managers.Select(m => m.Id));

            await _notificationService.CreateAsync(
                NotificationType.PositionCreated,
                "New Position Created",
                $"Position '{entity.PositionName}' has been created.",
                recipients
            );
        }

        public async Task DeleteAsync(int id, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to delete recruitment positions.");
            }
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
            {
                throw new InvalidOperationException("Recruitment position not found.");
            }

            var deleted = new DeletedRecruitmentPosition
            {
                Id = entity.Id,
                PositionName = entity.PositionName,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                DepartmentId = entity.DepartmentId,
                DeletedAt = DateTime.UtcNow.AddHours(7),
                DeletedById = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier))
            };

            await _repo.DeleteWithArchiveAsync(entity, deleted);
            await _repo.SaveChangesAsync();
        }

        public async Task<IEnumerable<RecruitmentPositionDto>> GetAllAsync(ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR") && !currentRole.Contains("Manager"))
            {
                throw new UnauthorizedAccessException("You do not have permission to view recruitment positions.");
            }

            var positions = await _repo.GetAllAsync();
            if (positions == null || !positions.Any())
            {
                throw new InvalidOperationException("No recruitment positions found.");
            }
            return _mapper.Map<IEnumerable<RecruitmentPositionDto>>(positions);
        }

        public async Task<RecruitmentPositionDto?> GetByIdAsync(int id, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR") && !currentRole.Contains("Manager"))
            {
                throw new UnauthorizedAccessException("You do not have permission to view recruitment positions.");
            }

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
            {
                throw new InvalidOperationException("Recruitment position not found.");
            }
            return _mapper.Map<RecruitmentPositionDto>(entity);
        }

        public async Task UpdateAsync(RecruitmentPositionDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to update recruitment positions.");
            }
            var entity = _mapper.Map<RecruitmentPosition>(dto);
            if (entity == null)
            {
                throw new InvalidOperationException("Recruitment position not found.");
            }

            _repo.Update(entity);
            await _repo.SaveChangesAsync();
        }

        public async Task<IEnumerable<RecruitmentPositionDto>> GetByManagerDepartmentAsync(ClaimsPrincipal user)
        {
            var empId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));

            var employee = await _employeeRepo.GetByIdAsync(empId);
            if (employee == null)
                throw new InvalidOperationException("Employee not found.");
            if (employee.DepartmentId == null)
                throw new InvalidOperationException("Manager has no department.");
            Console.WriteLine($"[DEBUG] EmployeeId = {empId}");
            Console.WriteLine($"[DEBUG] Employee.DepartmentId = {employee.DepartmentId}");

            var positions = await _repo.GetAllAsync(
                p => p.DepartmentId == employee.DepartmentId
            );

            return _mapper.Map<IEnumerable<RecruitmentPositionDto>>(positions);
        }


    }
}
