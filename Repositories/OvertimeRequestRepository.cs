using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class OvertimeRequestRepository : BaseRepository<OvertimeRequest, DeletedOvertimeRequest>, IOvertimeRequestRepository
    {
        public OvertimeRequestRepository(AppDbContext dbContext, DeletedDbContext deletedContext) : base(dbContext, deletedContext)
        {
        }
        public async Task<IEnumerable<OvertimeRequest>> GetAllAsync()
        {
            return await _context.OvertimeRequests
                .Include(r => r.Employees)
                .Include(r => r.ApprovedBy)
                .ToListAsync();
        }

        public async Task<OvertimeRequest?> GetByIdAsync(int id)
        {
            return await _context.OvertimeRequests
                .Include(r => r.Employees)
                .Include(r => r.ApprovedBy)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<OvertimeRequest>> GetApprovedByDateAsync(int employeeId, DateTime date)
        {
            DateTime start = date.Date;
            DateTime end = date.Date.AddDays(1);

            return await _context.OvertimeRequests
                .Include(r => r.Employees)
                .Include(r => r.ApprovedBy)
                .Where(r =>
                    r.EmployeeId == employeeId &&
                    r.Status == OvertimeStatus.Approved &&
                    r.StartTime >= start &&
                    r.EndTime < end)
                .ToListAsync();
        }
    }
}
