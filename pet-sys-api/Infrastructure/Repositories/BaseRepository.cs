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

        protected virtual IQueryable<T> Query => _context.Set<T>();

        public async Task<T?> GetByIdAsync(int id)
        {
            return await Query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate)
        {
            return await Query.Where(predicate).ToListAsync();
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
            return await Paginate(_context.Set<T>().OrderBy(e => e.Id), pageIndex, pageSize);
        }

        protected async Task<PaginatedList<T>> Paginate(IQueryable<T> query, int pageIndex, int pageSize)
        {
            var count = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)count / pageSize);
            return new PaginatedList<T>(items, pageIndex, totalPages);
        }

        protected static IQueryable<T> ApplyOrder<TKey>(IQueryable<T> query, Expression<Func<T, TKey>> key, bool desc)
        {
            return desc
                ? query.OrderByDescending(key).ThenByDescending(e => e.Id)
                : query.OrderBy(key).ThenBy(e => e.Id);
        }
    }
}
