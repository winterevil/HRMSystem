using System.Security.Claims;
using AutoMapper;
using HRMSystem.DTOs;
using HRMSystem.Models;
using HRMSystem.Repositories;

namespace HRMSystem.Services
{
    public class RecruitmentRequirementService:IRecruitmentRequirementService
    {
        private readonly IRecruitmentRequirementRepository _repo;
        private readonly IMapper _mapper;
        public RecruitmentRequirementService(IRecruitmentRequirementRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task ApproveAsync(int id, RecruitmentStatus status, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to approve recruitment requirements.");
            }

            var entity = await _repo.GetByIdAsync(id); 
            if (entity == null)
            {
                throw new InvalidOperationException("Recruitment requirement not found.");
            }

            if (entity.Status != RecruitmentStatus.Pending)
            {
                throw new InvalidOperationException("Recruitment requirement is not in a state that can be updated.");
            }
            entity.Status = status;
            _repo.Update(entity);
            await _repo.SaveChangesAsync();
        }

        public async Task CreateAsync(RecruitmentRequirementDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("Manager"))
            {
                throw new UnauthorizedAccessException("You do not have permission to create recruitment requirements.");
            }
            var positionId = dto.PositionId;
            if (positionId <= 0)
            {
                throw new InvalidOperationException("Invalid position ID provided.");
            }
            var entity = _mapper.Map<RecruitmentRequirement>(dto);
            if (entity == null)
            {
                throw new InvalidOperationException("Failed to create recruitment requirement.");
            }
            entity.CreatedAt = DateTime.UtcNow.AddHours(7);
            entity.EmployeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
            entity.Status = RecruitmentStatus.Pending;
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
        }

        public Task DeleteAsync(int id, ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<RecruitmentRequirementDto>> GetAllAsync(ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            IEnumerable<RecruitmentRequirement> requirements;
            if (currentRole.Contains("HR")) 
            { 
                requirements = await _repo.GetAllAsync();
            }
            else if (currentRole.Contains("Manager"))
            {
                requirements = await _repo.GetAllAsync();
                int currentEmployeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
                requirements = requirements.Where(r => r.EmployeeId == currentEmployeeId);
            }
            else
            {
                throw new UnauthorizedAccessException("You do not have permission to view recruitment requirements.");
            }

            if (requirements == null || !requirements.Any())
            {
                throw new InvalidOperationException("No recruitment requirements found.");
            }
            return _mapper.Map<IEnumerable<RecruitmentRequirementDto>>(requirements);
        }

        public async Task<RecruitmentRequirementDto?> GetByIdAsync(int id, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            RecruitmentRequirement? requirement;
            if (currentRole.Contains("HR"))
            {
                requirement = await _repo.GetByIdAsync(id);
            }
            else if (currentRole.Contains("Manager"))
            {
                requirement = await _repo.GetByIdAsync(id);
                int currentEmployeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
                if (requirement != null && requirement.Employees.Id != currentEmployeeId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to view this recruitment requirement.");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("You do not have permission to view recruitment requirements.");
            }
            if (requirement == null)
            {
                throw new InvalidOperationException("Recruitment requirement not found.");
            }
            return _mapper.Map<RecruitmentRequirementDto>(requirement);
        }

        public async Task UpdateAsync(RecruitmentRequirementDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("Manager"))
            {
                throw new UnauthorizedAccessException("You do not have permission to update recruitment requirements.");
            }
            var entity = await _repo.GetByIdAsync(dto.Id);
            if (entity == null)
            {
                throw new InvalidOperationException("Recruitment requirement not found.");
            }
            if (entity.Status != RecruitmentStatus.Pending)
            {
                throw new InvalidOperationException("Recruitment requirement is not in a state that can be updated.");
            }
            _mapper.Map(dto, entity);
            entity.EmployeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
            entity.Status = RecruitmentStatus.Pending; 
            _repo.Update(entity);
            await _repo.SaveChangesAsync();
        }
    }
}
