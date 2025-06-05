using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class RecruitmentPositionRepository : BaseRepository<RecruitmentPosition, DeletedRecruitmentPosition>, IRecruimentPositionRepository
    {
        public RecruitmentPositionRepository(AppDbContext context, DeletedDbContext deletedContext) : base(context, deletedContext)
        {
        }
        public async Task<IEnumerable<RecruitmentPosition>> GetAllAsync()
        {
            return await _context.RecruitmentPositions
                                 .Include(rp => rp.Departments)
                                 .ToListAsync();
        }
        public async Task<RecruitmentPosition?> GetByIdAsync(int id)
        {
            return await _context.RecruitmentPositions
                                 .Include(rp => rp.Departments)
                                 .FirstOrDefaultAsync(rp => rp.Id == id);
        }

    }

}
