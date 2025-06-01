using HRMSystem.Data;
using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public class EmployeeTypeRepository : BaseRepository<EmployeeType, DeletedEmployeeType>, IEmployeeTypeRepository
    {
        public EmployeeTypeRepository(AppDbContext context, DeletedDbContext deletedContext) : base(context, deletedContext)
        {
        }
    }
}
