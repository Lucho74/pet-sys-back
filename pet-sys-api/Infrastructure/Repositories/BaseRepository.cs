using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly List<T> _items = new();

        public Task<T> GetByIdAsync(string id)
        {
            var idProperty = typeof(T).GetProperty("Id");
            var entity = _items.FirstOrDefault(e => (string)idProperty.GetValue(e) == id);
            return Task.FromResult(entity);
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(_items.AsEnumerable());
        }

        public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var compiled = predicate.Compile();
            var result = _items.Where(compiled);
            return Task.FromResult(result);
        }

        public Task<T> AddAsync(T entity)
        {
            _items.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(T entity)
        {
            var idProperty = typeof(T).GetProperty("Id");
            var id = (string)idProperty.GetValue(entity);
            var index = _items.FindIndex(e => (string)idProperty.GetValue(e) == id);
            if (index != -1)
                _items[index] = entity;
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
                _items.Remove(entity);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var entity = await GetByIdAsync(id);
            return entity != null;
        }
    }
}
