using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly List<T> _items = new();

        private static readonly PropertyInfo IdProperty = typeof(T).GetProperty("Id")
            ?? throw new InvalidOperationException($"{typeof(T).Name} does not have an 'Id' property.");

        public Task<T> GetByIdAsync(int id)
        {
            var entity = FindById(id);
            if (entity == null)
                throw new KeyNotFoundException($"{typeof(T).Name} with Id {id} was not found.");
            return Task.FromResult(entity);
        }

        private T? FindById(int id)
        {
            return _items.FirstOrDefault(e => (int)IdProperty.GetValue(e)! == id);
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
            var id = (int)IdProperty.GetValue(entity)!;
            var index = _items.FindIndex(e => (int)IdProperty.GetValue(e)! == id);
            if (index != -1)
                _items[index] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var entity = FindById(id);
            if (entity != null)
                _items.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(int id)
        {
            return Task.FromResult(FindById(id) != null);
        }
    }
}
