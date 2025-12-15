using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class EmployeeTypeRepository : BaseRepository<EmployeeType, DeletedEmployeeType>, IEmployeeTypeRepository
    {
        public EmployeeTypeRepository(AppDbContext context, DeletedDbContext deletedContext) : base(context, deletedContext)
        {
        }
        public async Task<int> CountEmployeesByTypeAsync(int employeeTypeId)
        {
            return await _context.Employees
                .CountAsync(e => e.EmployeeTypeId == employeeTypeId);
        }
    }
}
