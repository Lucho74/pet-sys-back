using Domain.Common;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task<PaginatedList<User>> GetPagedAsync(UserQuery q, IReadOnlyCollection<string> readableRoles);
    }
}
