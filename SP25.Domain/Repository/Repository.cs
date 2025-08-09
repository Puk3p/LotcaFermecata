using Microsoft.EntityFrameworkCore;
using SP25.Domain.Context;
using System.Linq.Expressions;

namespace SP25.Domain.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly MyDbContext _context;
        private readonly DbSet<T> _entities;

        public Repository(MyDbContext context)
        {
            _context = context;
            _entities = context.Set<T>();
        }

        // --------- Sync (existente) ----------
        public T GetById(Guid id) => _entities.Find(id);

        public IEnumerable<T> GetAll() => _entities.ToList();

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
            => _entities.Where(predicate).ToList();

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _entities;
            foreach (var includeProperty in includeProperties)
                query = query.Include(includeProperty);

            return query.Where(predicate).ToList();
        }

        public void Add(T entity) => _entities.Add(entity);

        public void Update(T entity) => _entities.Update(entity);

        public void Remove(T entity) => _entities.Remove(entity);

        public void SaveChanges() => _context.SaveChanges();

        public async Task<IEnumerable<T>> GetAllAsync() => await _entities.ToListAsync();

        public async Task<T?> GetByIdAsync(Guid id)
            => await _entities.FindAsync(id);

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _entities.Where(predicate).ToListAsync();

        public async Task AddAsync(T entity)
            => await _entities.AddAsync(entity);

        public Task UpdateAsync(T entity)
        {
            _entities.Update(entity);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(T entity)
        {
            _entities.Remove(entity);
            return Task.CompletedTask;
        }
        public Task DeleteAsync(T entity)
        {
            _entities.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
