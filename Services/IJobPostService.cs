using System.Security.Claims;
using HRMSystem.DTOs;

namespace HRMSystem.Services
{
    public interface IJobPostService
    {
        Task<IEnumerable<JobPostDto>> GetAllAsync();
        Task<JobPostDto?> GetByIdAsync(int id);
        Task CreateAsync(JobPostDto dto, ClaimsPrincipal user);
        Task UpdateAsync(JobPostDto dto, ClaimsPrincipal user);
        Task DeleteAsync(int id, ClaimsPrincipal user);
    }
}
