using Application.Models;
using Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserServices
    {
        Task<IEnumerable<UserDTO>> GetAllUserAsync(IReadOnlyCollection<string> readableRoles);
        Task<UserDTO> GetUserByIdAsync(int id);
        Task<UserDTO> AddUserAsync(CreateUserDTO dto);
        Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO dto);
        Task UpdateUserRoleAsync(int id, UpdateUserRoleDTO dto);
        Task DeleteUserAsync(int id);
        Task ReactivateUserAsync(int id);
        Task<UserDTO> AuthenticateAsync(LoginDTO dto);
        Task<PaginatedList<UserDTO>> GetUsersPagedAsync(UserQuery q, IReadOnlyCollection<string> readableRoles);
    }
}
