using Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserServices
    {
        Task<IEnumerable<UserDTO>> GetAllUserAsync(int callerId, string callerRole);
        Task<UserDTO> GetUserByIdAsync(int id, int callerId, string callerRole);
        Task<UserDTO> AddUserAsync(CreateUserDTO dto, string callerRole);
        Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO dto, int callerId, string callerRole);
        Task DeleteUserAsync(int id, int callerId, string callerRole);
        Task ReactivateUserAsync(int id, int callerId, string callerRole);
        Task<UserDTO> AuthenticateAsync(LoginDTO dto);
    }
}
