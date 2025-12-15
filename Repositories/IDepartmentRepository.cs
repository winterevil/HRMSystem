using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface IDepartmentRepository : IBaseRepository<Department, DeletedDepartment>
    {
        Task<int> CountEmployeesAsync(int deptId);
    }
}
