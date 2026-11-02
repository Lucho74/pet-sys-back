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
    }
}
