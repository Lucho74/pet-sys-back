using Domain.Common;
using Domain.Entities;
using Domain.Enums;
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
                .Include(u => u.ClientProfile)
                .Include(u => u.VeterinarianProfile);

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await Query.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<PaginatedList<User>> GetPagedAsync(UserQuery filters, IReadOnlyCollection<UserRole> readableRoles)
        {
            var allowedRoles = readableRoles.ToArray();

            var query = Query.Where(user => allowedRoles.Contains(user.Role));

            if (filters.IsSuspend.HasValue)
            {
                var isSuspended = filters.IsSuspend.Value;
                query = query.Where(user => user.IsSuspended == isSuspended);
            }
            else if (!filters.IncludeSuspended)
            {
                query = query.Where(user => !user.IsSuspended);
            }

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var pattern = $"%{filters.Search.Trim()}%";
                query = query.Where(user => EF.Functions.Like(user.FullName, pattern) || EF.Functions.Like(user.Email, pattern));
            }

            if(filters.Role.HasValue)
            {
                var role = filters.Role.Value;
                query = query.Where(user => user.Role == role);
            }

            query = filters.SortBy?.ToLowerInvariant() switch
            {
                "fullname" => ApplyOrder(query, user => user.FullName, filters.SortDesc),
                "email" => ApplyOrder(query, user => user.Email, filters.SortDesc),
                "role" => ApplyOrder(query, user => user.Role == UserRole.Client ? 2
                    : user.Role == UserRole.Veterinarian ? 1 : 0, filters.SortDesc),
                _ => ApplyOrder(query, user => user.Id, filters.SortDesc)
            };

            return await Paginate(query, filters.PageIndex, filters.PageSize);
        }

        public async Task SetSuspendedAsync(int userId, bool isSuspended)
        {
            var user = await _context.Users.FirstAsync(u => u.Id == userId);
            var pets = await _context.Pets
                .Include(p => p.Consultations)
                .Where(p => p.ClientId == userId)
                .ToListAsync();

            user.IsSuspended = isSuspended;
            foreach (var pet in pets)
            {
                if (pet.IsSuspended == pet.SuspendedWithOwner)
                {
                    pet.IsSuspended = isSuspended;
                    pet.SuspendedWithOwner = isSuspended;
                }
                foreach (var consultation in pet.Consultations)
                {
                    if (consultation.IsSuspended == consultation.SuspendedWithOwner)
                    {
                        consultation.IsSuspended = isSuspended;
                        consultation.SuspendedWithOwner = isSuspended;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
