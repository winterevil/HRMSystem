﻿using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class LeaveRequestRepository : BaseRepository<LeaveRequest, DeletedLeaveRequest>, ILeaveRequestRepository
    {
        public LeaveRequestRepository(AppDbContext dbContext, DeletedDbContext deletedContext) : base(dbContext, deletedContext)
        {           
        }
        public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
        {
            return await _context.LeaveRequests
                .Include(r => r.Employees)
                .Include(r => r.ApprovedBy)
                .ToListAsync();
        }

        public async Task<LeaveRequest?> GetByIdAsync(int id)
        {
            return await _context.LeaveRequests
                .Include(r => r.Employees)
                .Include(r => r.ApprovedBy)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
