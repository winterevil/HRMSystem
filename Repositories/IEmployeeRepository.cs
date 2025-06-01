using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface IEmployeeRepository : IBaseRepository<Employee, DeletedEmployee>
    {
    }
}
