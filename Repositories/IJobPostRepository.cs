using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface IJobPostRepository:IBaseRepository<JobPost, DeletedJobPost>
    {
        Task<IEnumerable<JobPost>> GetAllAsync();
        Task<JobPost?> GetByIdAsync(int id);
    }
}
