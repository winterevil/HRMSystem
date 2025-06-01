using System.Security.Claims;
using HRMSystem.DTOs;

namespace HRMSystem.Services
{
    public interface IAttendanceService
    {
        Task CheckinAsync(ClaimsPrincipal user);
        Task CheckoutAsync(ClaimsPrincipal user);
        Task<IEnumerable<AttendanceDto>> GetAllAsync(ClaimsPrincipal user);
    }
}
