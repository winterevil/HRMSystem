using HRMSystem.Data;
using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public class RecruitmentRequirementRepository:BaseRepository<RecruitmentRequirement, DeletedRecruitmentRequirement>, IRecruitmentRequirementRepository
    {
        public RecruitmentRequirementRepository(AppDbContext context, DeletedDbContext deletedContext) : base(context, deletedContext)
        {
        }
    }

}
