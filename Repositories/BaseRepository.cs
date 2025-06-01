
using HRMSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class BaseRepository<T, TArchive> : IBaseRepository<T, TArchive> where T : class where TArchive:class
    {
        protected readonly AppDbContext _context;
        protected readonly DeletedDbContext _deletedContext;

        public BaseRepository(AppDbContext context, DeletedDbContext deletedContext)
        {
            _context = context;
            _deletedContext = deletedContext;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public async Task DeleteWithArchiveAsync(T entity, TArchive archiveEntity)
        {
            await _deletedContext.Set<TArchive>().AddAsync(archiveEntity);
            await _deletedContext.SaveChangesAsync();
            _context.Set<T>().Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
