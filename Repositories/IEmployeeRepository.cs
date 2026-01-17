using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface IEmployeeRepository : IBaseRepository<Employee, DeletedEmployee>
    {
        Task<List<Employee>> GetEmployeesByRoleAsync(string roleName);
        Task<List<Employee>> GetManagersByDepartmentAsync(int departmentId);
        Task<bool> HasRoleAsync(int employeeId, string roleName);
    }
}
