using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        protected override IQueryable<User> Query =>
            _context.Users
                .Include(u => u.Role)
                .Include(u => u.ClientProfile)
                .Include(u => u.VeterinarianProfile);

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await Query.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles
                .SingleOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task<PaginatedList<User>> GetPagedAsync(UserQuery filters, IReadOnlyCollection<string> readableRoles)
        {
            var allowedRoles = readableRoles.ToArray();

            var query = Query.Where(user => allowedRoles.Contains(user.Role.Name));

            if (!filters.IncludeDeleted)
                query = query.Where(user => !user.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var pattern = $"%{filters.Search.Trim()}%";
                query = query.Where(user => EF.Functions.Like(user.FullName, pattern) || EF.Functions.Like(user.Email, pattern));
            }

            if (!string.IsNullOrWhiteSpace(filters.Role))
            {
                var role = filters.Role.Trim();

                query = query.Where(user => user.Role.Name == role);
            }

            query = filters.SortBy?.ToLowerInvariant() switch
            {
                "fullname" => ApplyOrder(query, user => user.FullName, filters.SortDesc),
                "email" => ApplyOrder(query, user => user.Email, filters.SortDesc),
                _ => ApplyOrder(query, user => user.Id, filters.SortDesc)
            };

            return await Paginate(query, filters.PageIndex, filters.PageSize);
        }
    }
}
