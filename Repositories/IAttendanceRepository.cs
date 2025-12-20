using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface IAttendanceRepository:IBaseRepository<Attendance, DeletedAttendance>
    {
        Task<Attendance?> GetByDateAsync(int employeeId, DateTime date);
        Task<IEnumerable<Attendance>> GetAllWithEmployeesAsync();
        Task<IEnumerable<Attendance>> GetPendingCheckoutsByDateAsync(DateTime today);
    }
}
