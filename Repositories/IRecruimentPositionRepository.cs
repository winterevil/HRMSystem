using System.Linq.Expressions;
using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface IRecruimentPositionRepository : IBaseRepository<RecruitmentPosition, DeletedRecruitmentPosition>
    {
        //void AttachDepartment(int departmentId);
        Task<IEnumerable<RecruitmentPosition>> GetAllAsync();
        Task<RecruitmentPosition?> GetByIdAsync(int id);
        Task<IEnumerable<RecruitmentPosition>> GetAllAsync(Expression<Func<RecruitmentPosition, bool>> predicate);

    }
}
