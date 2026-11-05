using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces;
using Infrastructure.Context;
using System.Text.RegularExpressions;
using Domain.Common;

namespace Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class, IEntity
    {
        public readonly ApplicationDbContext _context;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            var entity = await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id);
            return entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T?> UpdateAsync(int id, T entity)
        {
            var existingEntity = await _context.Set<T>().FindAsync(id);
            if (existingEntity == null)
            {
                return null;
            }

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existingEntity;
        }

        public async Task<PaginatedList<T>> ListAllByPagination(int pageIndex, int pageSize)
        {
            var items = await _context.Set<T>()
                .OrderBy(b => b.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var count = await _context.Set<T>().CountAsync();
            var totalPages = (int)Math.Ceiling((double)count / pageSize);
            return new PaginatedList<T>(items, pageIndex, totalPages);
        }
    }
}
