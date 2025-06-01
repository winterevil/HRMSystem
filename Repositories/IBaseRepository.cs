namespace HRMSystem.Repositories
{
    public interface IBaseRepository<T, TArchive> where T:class where TArchive:class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        Task DeleteWithArchiveAsync(T entity, TArchive archiveEntity);
        Task SaveChangesAsync();
    }
}
