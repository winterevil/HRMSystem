using System.Security.Claims;
using HRMSystem.DTOs;
using HRMSystem.Models;

namespace HRMSystem.Services
{
    public interface ILeaveRequestService
    {
        Task<IEnumerable<LeaveRequestDto>> GetAllAsync(ClaimsPrincipal user);
        Task<LeaveRequestDto?> GetByIdAsync(int id, ClaimsPrincipal user);
        Task CreateAsync(LeaveRequestDto dto, ClaimsPrincipal user);
        Task UpdateAsync(LeaveRequestDto dto, ClaimsPrincipal user);
        Task ApproveAsync(int id, LeaveStatus status, ClaimsPrincipal user);
        Task DeleteAsync(int id, ClaimsPrincipal user);
    }
}
