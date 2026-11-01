using Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserServices
    {
        Task<IEnumerable<UserDTO>> GetAllUserAsync(IReadOnlyCollection<string> readableRoles);
        Task<UserDTO> GetUserByIdAsync(int id);
        Task<IEnumerable<ClientProfileDTO>> GetClientProfilesAsync();
        Task<ClientProfileDTO> GetClientProfileByUserIdAsync(int userId);
        Task<UserDTO> AddUserAsync(CreateUserDTO dto);
        Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO dto);
        Task UpdateUserRolesAsync(int id, UpdateUserRolesDTO dto);
        Task DeleteUserAsync(int id);
        Task ReactivateUserAsync(int id);
        Task<UserDTO> AuthenticateAsync(LoginDTO dto);
    }
}
