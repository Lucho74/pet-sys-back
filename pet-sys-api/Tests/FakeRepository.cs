using System.Linq.Expressions;
using Domain.Interfaces;

namespace Tests
{
    public class FakeRepository<T> : IBaseRepository<T> where T : class, IEntity
    {
        public List<T> Items { get; } = new();

        public Task<T?> GetByIdAsync(int id) => Task.FromResult(Items.FirstOrDefault(e => e.Id == id));
        public Task<IEnumerable<T>> GetAllAsync() => Task.FromResult<IEnumerable<T>>(Items.ToList());
        public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => Task.FromResult<IEnumerable<T>>(Items.Where(predicate.Compile()).ToList());
        public Task<bool> ExistsAsync(int id) => Task.FromResult(Items.Any(e => e.Id == id));

        public Task<T> AddAsync(T entity)
        {
            entity.Id = Items.Count == 0 ? 1 : Items.Max(e => e.Id) + 1;
            Items.Add(entity);
            return Task.FromResult(entity);
        }

        // Los servicios devuelven la misma instancia que leyeron, así que no hay nada que copiar.
        public Task<T?> UpdateAsync(int id, T entity) => Task.FromResult(Items.FirstOrDefault(e => e.Id == id));

        public Task DeleteAsync(int id)
        {
            Items.RemoveAll(e => e.Id == id);
            return Task.CompletedTask;
        }
    }
}
