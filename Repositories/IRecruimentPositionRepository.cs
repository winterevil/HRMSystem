using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface IRecruimentPositionRepository : IBaseRepository<RecruitmentPosition, DeletedRecruitmentPosition>
    {
        void AttachDepartment(int departmentId);
        Task<IEnumerable<RecruitmentPosition>> GetAllAsync();
        Task<RecruitmentPosition?> GetByIdAsync(int id);
    }
}
