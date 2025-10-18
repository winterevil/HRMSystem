using HRMSystem.Data;
using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public class DepartmentRepository : BaseRepository<Department, DeletedDepartment>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context, DeletedDbContext deletedContext) : base(context, deletedContext)
        {
        }
    }
}
