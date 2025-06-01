using System.Security.Claims;
using HRMSystem.DTOs;

namespace HRMSystem.Services
{
    public interface IEmployeeTypeService
    {
        Task<IEnumerable<EmployeeTypeDto>> GetAllAsync(ClaimsPrincipal user);
        Task<EmployeeTypeDto?> GetByIdAsync(int id, ClaimsPrincipal user);
        Task CreateAsync(EmployeeTypeDto dto, ClaimsPrincipal user);
        Task UpdateAsync(EmployeeTypeDto dto, ClaimsPrincipal user);
        Task DeleteAsync(int id, ClaimsPrincipal user);
    }
}
