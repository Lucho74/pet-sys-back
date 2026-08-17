using Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserServices
    {
        Task<IEnumerable<UserDTO>> GetAllUserAsync();
        Task<UserDTO> GetUserByIdAsync(int id);
        Task<UserDTO> AddUserAsync(CreateUserDTO dto);
        Task<UserDTO> UpdateUserAsync(int id, UserDTO dto);
        Task DeleteUserAsync(int id);
    }
}
