using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee, DeletedEmployee>, IEmployeeRepository
    {
        public EmployeeRepository(AppDbContext context, DeletedDbContext deletedContext) : base(context, deletedContext)
        {
        }

        public new async Task<IEnumerable<Employee>> GetAllAsync() => await _context.Employees
            .Include(e => e.EmployeeRoles)
            .ThenInclude(er => er.Roles)
            .Include(e => e.Departments)
            .Include(e => e.EmployeeTypes)
            .ToListAsync();

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Roles)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
        public async Task<List<Employee>> GetEmployeesByRoleAsync(string roleName)
        {
            return await _context.Employees
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Roles)
                .Include(e => e.Departments)         
                .Where(e =>
                    e.EmployeeRoles.Any(er => er.Roles.RoleName == roleName))
                .ToListAsync();
        }

        public async Task<List<Employee>> GetManagersByDepartmentAsync(int departmentId)
        {
            return await _context.Employees
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Roles)
                .Where(e =>
                    e.DepartmentId == departmentId &&
                    e.EmployeeRoles.Any(er => er.Roles.RoleName == "Manager"))
                .ToListAsync();
        }
        public async Task<bool> HasRoleAsync(int employeeId, string roleName)
        {
            return await _context.EmployeeRoles
                .AnyAsync(er =>
                    er.EmployeeId == employeeId &&
                    er.Roles.RoleName == roleName);
        }

    }

}
