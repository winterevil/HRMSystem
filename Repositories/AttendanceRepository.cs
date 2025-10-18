using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class AttendanceRepository : BaseRepository<Attendance, DeletedAttendance>, IAttendanceRepository
    {
        public AttendanceRepository(AppDbContext context, DeletedDbContext deletedContext) : base(context, deletedContext)
        {
        }

        public async Task<IEnumerable<Attendance>> GetAllWithEmployeesAsync()
        {
            return await _context.Attendance
                .Include(a => a.Employees)
                .ToListAsync();
        }

        public async Task<Attendance?> GetByDateAsync(int employeeId, DateTime date)
        {
            return await _context.Attendance
                .Include(a => a.Employees)
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.CheckinDate == date);
        }

        public async Task<IEnumerable<Attendance>> GetPendingCheckoutsBeforeAsync(DateTime date)
        {
            return await _context.Attendance
                .Where(a => a.CheckoutTime == null && a.CheckinDate < date)
                .ToListAsync();
        }

    }

}
