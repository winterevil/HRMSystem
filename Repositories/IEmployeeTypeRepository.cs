using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface IEmployeeTypeRepository : IBaseRepository<EmployeeType, DeletedEmployeeType>
    {
        Task<int> CountEmployeesByTypeAsync(int employeeTypeId);
    }
}
