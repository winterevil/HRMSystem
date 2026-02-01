using System.Security.Claims;
using HRMSystem.DTOs;

namespace HRMSystem.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetAllAsync(EmployeeDto dto, ClaimsPrincipal user);
        Task<EmployeeDto?> GetByIdAsync(int id, EmployeeDto dto, ClaimsPrincipal user);
        Task CreateAsync(EmployeeCreateDto dto, ClaimsPrincipal user);
        Task UpdateAsync(EmployeeUpdateDto dto, ClaimsPrincipal user);
        Task DeleteAsync(int id, EmployeeDto dto, ClaimsPrincipal user);
        Task<IEnumerable<EmployeeWithDeletedDto>> GetAllIncludeDeletedAsync(ClaimsPrincipal user);
        Task RestoreAsync(int employeeId, ClaimsPrincipal user);
    }
}
