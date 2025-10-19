using System.Security.Claims;
using HRMSystem.DTOs;
namespace HRMSystem.Services
{
    public interface IRecruitmentPositionService
    {
        Task<IEnumerable<RecruitmentPositionDto>> GetAllAsync(ClaimsPrincipal user);
        Task<RecruitmentPositionDto?> GetByIdAsync(int id, ClaimsPrincipal user);
        Task CreateAsync(RecruitmentPositionDto dto, ClaimsPrincipal user);
        Task UpdateAsync(RecruitmentPositionDto dto, ClaimsPrincipal user);
        Task DeleteAsync(int id, ClaimsPrincipal user);
        Task<IEnumerable<RecruitmentPositionDto>> GetByManagerDepartmentAsync(ClaimsPrincipal user);
    }
}
