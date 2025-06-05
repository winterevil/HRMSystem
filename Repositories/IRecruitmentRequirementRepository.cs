using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface IRecruitmentRequirementRepository:IBaseRepository<RecruitmentRequirement, DeletedRecruitmentRequirement>
    {
        Task<RecruitmentRequirement?> GetByIdAsync(int positionId);
        Task<IEnumerable<RecruitmentRequirement>> GetAllAsync();
    }
}
