using Domain.Common;
using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<PaginatedList<User>> GetPagedAsync(UserQuery q, IReadOnlyCollection<UserRole> readableRoles);
        Task SetSuspendedAsync(int userId, bool isSuspended);
    }
}
