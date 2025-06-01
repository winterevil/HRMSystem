using System.Security.Claims;
using HRMSystem.DTOs;
using HRMSystem.Models;

namespace HRMSystem.Services
{
    public interface IOvertimeRequestService
    {
        Task<IEnumerable<OvertimeRequestDto>> GetAllAsync(ClaimsPrincipal user);
        Task<OvertimeRequestDto?> GetByIdAsync(int id, ClaimsPrincipal user);
        Task CreateAsync(OvertimeRequestDto dto, ClaimsPrincipal user);
        Task UpdateAsync(OvertimeRequestDto dto, ClaimsPrincipal user);
        Task ApproveAsync(int id, OvertimeStatus status, ClaimsPrincipal user);
        Task DeleteAsync(int id, ClaimsPrincipal user);
    }
}
