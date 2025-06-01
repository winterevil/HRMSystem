using System.Security.Claims;
using HRMSystem.DTOs;
using HRMSystem.Models;

namespace HRMSystem.Services
{
    public interface IRecruitmentRequirementService
    {
        Task<IEnumerable<RecruitmentRequirementDto>> GetAllAsync(ClaimsPrincipal user);
        Task<RecruitmentRequirementDto?> GetByIdAsync(int id, ClaimsPrincipal user);
        Task CreateAsync(RecruitmentRequirementDto dto, ClaimsPrincipal user);
        Task UpdateAsync(RecruitmentRequirementDto dto, ClaimsPrincipal user);
        Task ApproveAsync(int id, RecruitmentStatus status, ClaimsPrincipal user);
        Task DeleteAsync(int id, ClaimsPrincipal user);
    }
}
