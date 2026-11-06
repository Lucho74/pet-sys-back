using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Set<User>().FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<IEnumerable<T>> GetByRoleAsync<T>() where T : User
        {
            return await _context.Set<User>().OfType<T>().ToListAsync();
        }

        public async Task<PaginatedList<User>> GetPagedAsync(UserQuery q)
        {
            var query = _context.Set<User>().AsQueryable();

            if (!q.IncludeDeleted)
                query = query.Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var pattern = $"%{q.Search.Trim()}%";
                query = query.Where(u => EF.Functions.Like(u.FullName, pattern) || EF.Functions.Like(u.Email, pattern));
            }

            if (q.Role.HasValue)
            {
                query = q.Role.Value switch
                {
                    UserType.Admin => query.Where(u => u is Admin),
                    UserType.Client => query.Where(u => u is Client),
                    UserType.Veterinarian => query.Where(u => u is Veterinarian),
                    _ => query
                };
            }

            query = q.SortBy?.ToLowerInvariant() switch
            {
                "fullname" => ApplyOrder(query, u => u.FullName, q.SortDesc),
                "email" => ApplyOrder(query, u => u.Email, q.SortDesc),
                _ => ApplyOrder(query, u => u.Id, q.SortDesc)
            };

            return await Paginate(query, q.PageIndex, q.PageSize);
        }
    }
}
