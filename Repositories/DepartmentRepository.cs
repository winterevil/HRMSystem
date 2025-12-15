using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class DepartmentRepository : BaseRepository<Department, DeletedDepartment>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context, DeletedDbContext deletedContext) : base(context, deletedContext)
        {
        }
        public async Task<int> CountEmployeesAsync(int departmentId)
        {
            return await _context.Employees.CountAsync(e => e.DepartmentId == departmentId);
        }

    }
}
