using System.Linq.Expressions;
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
        public async Task<IEnumerable<RecruitmentPosition>> GetAllAsync(
    Expression<Func<RecruitmentPosition, bool>> predicate)
        {
            var query = _context.RecruitmentPositions
                .Include(p => p.Departments)
                .Where(predicate);

            Console.WriteLine("[DEBUG] SQL = " + query.ToQueryString());
            return await query.ToListAsync();
        }

    }

}
