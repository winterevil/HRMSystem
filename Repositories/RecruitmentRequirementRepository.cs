using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class RecruitmentRequirementRepository : BaseRepository<RecruitmentRequirement, DeletedRecruitmentRequirement>, IRecruitmentRequirementRepository
    {
        public RecruitmentRequirementRepository(AppDbContext context, DeletedDbContext deletedContext) : base(context, deletedContext)
        {

        }
        public async Task<IEnumerable<RecruitmentRequirement>> GetAllAsync()
        {
            return await _context.RecruitmentRequirements
                                 .Include(rr => rr.RecruitmentPositions)
                                 .Include(e => e.Employees)
                                 .ToListAsync();
        }
        public async Task<RecruitmentRequirement?> GetByIdAsync(int id)
        {
            return await _context.RecruitmentRequirements
                                 .Include(rr => rr.RecruitmentPositions)
                                 .Include(e => e.Employees)
                                 .FirstOrDefaultAsync(rr => rr.Id == id);
        }
    }

}
