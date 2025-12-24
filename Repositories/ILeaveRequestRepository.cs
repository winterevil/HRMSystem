using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface ILeaveRequestRepository:IBaseRepository<LeaveRequest, DeletedLeaveRequest>
    {
        Task<IEnumerable<LeaveRequest>> GetAllAsync();
        Task<LeaveRequest?> GetByIdAsync(int id);
        Task<IEnumerable<LeaveRequest>> GetByEmployeeAndDateAsync(int employeeId, DateTime date);
        Task<IEnumerable<LeaveRequest>> GetApprovedLeavesByEmployeeIdAsync(int employeeId);

    }
}
