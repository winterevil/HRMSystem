using System.Security.Claims;
using HRMSystem.DTOs;

namespace HRMSystem.Services
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetAllAsync(ClaimsPrincipal user);
        Task<DepartmentDto?> GetByIdAsync(int id, ClaimsPrincipal user);
        Task CreateAsync(DepartmentDto dto, ClaimsPrincipal user);
        Task UpdateAsync(DepartmentDto dto, ClaimsPrincipal user);
        Task DeleteAsync(int id, ClaimsPrincipal user);
    }
}
