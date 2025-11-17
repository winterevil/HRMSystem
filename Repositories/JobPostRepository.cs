using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class JobPostRepository : BaseRepository<JobPost, DeletedJobPost>, IJobPostRepository
    {
        public JobPostRepository(AppDbContext context, DeletedDbContext deletedContext) : base(context, deletedContext)
        {
        }
        public async Task<IEnumerable<JobPost>> GetAllAsync()
        {
            return await _context.JobPosts
                .Include(jp => jp.RecruitmentRequirements)
                .Include(jp => jp.PostedBy)
                .ToListAsync();
        }
        public async Task<JobPost?> GetByIdAsync(int id)
        {
            return await _context.JobPosts
                .Include(jp => jp.RecruitmentRequirements)
                .Include(jp => jp.PostedBy)
                .FirstOrDefaultAsync(jp => jp.Id == id);
        }
        public async Task<IEnumerable<JobPost>> GetByRequirementIdAsync(int requirementId)
        {
            return await _context.JobPosts
                .Where(jp => jp.RequirementId == requirementId)
                .ToListAsync();
        }
    }
}
