using Application.Exceptions;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserServices : IUserServices
    {
        public readonly IUserRepository _userRepository;

        public UserServices(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUserAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var userList = users.ToList();
            if (userList.Count == 0)
            {
                throw new NotFoundException("No users found.");
            }

            return userList.Select(user => new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsDeleted = user.IsDeleted,
                RoleName = user.GetType().Name,
                Dni = user is Client client ? client.Dni : null
            });
        }

        public async Task<UserDTO> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new NotFoundException($"User with id {id} was not found.");
            }

            return new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsDeleted = user.IsDeleted,
                RoleName = user.GetType().Name,
            };
        }

        public async Task<UserDTO> AddUserAsync(CreateUserDTO dto)
        {
            User newUser;
            switch (dto.UserType)
            {
                case UserType.Veterinarian:
                    newUser = new Veterinarian
                    {
                        FullName = dto.FullName,
                        Email = dto.Email,
                        Phone = dto.Phone,
                        Password = dto.Password ?? string.Empty
                    };
                    break;
                case UserType.Admin:
                    newUser = new Admin
                    {
                        FullName = dto.FullName,
                        Email = dto.Email,
                        Phone = dto.Phone,
                        Password = dto.Password ?? string.Empty
                    };
                    break;
                case UserType.Client:
                default:
                    newUser = new Client
                    {
                        FullName = dto.FullName,
                        Email = dto.Email,
                        Phone = dto.Phone,
                        Password = dto.Password ?? string.Empty,
                        Dni = dto.Dni ?? string.Empty
                    };
                    break;
            }

            newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);

            var created = await _userRepository.AddAsync(newUser);
            return new UserDTO
            {
                Id = created.Id,
                FullName = created.FullName,
                Email = created.Email,
                Phone = created.Phone,
                IsDeleted = created.IsDeleted,
                RoleName = created.GetType().Name,

            };
        }

        public async Task<UserDTO> UpdateUserAsync(int id, UserDTO dto)
        {
            var existing = await _userRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new NotFoundException($"User with id {id} was not found.");
            }

            existing.FullName = dto.FullName;
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                existing.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            var updated = await _userRepository.UpdateAsync(id, existing);
            if (updated == null)
            {
                throw new NotFoundException($"User with id {id} was not found.");
            }

            return new UserDTO
            {
                Id = updated.Id,
                FullName = updated.FullName,
                Email = updated.Email,
                Phone = updated.Phone,
                IsDeleted = updated.IsDeleted,
                RoleName = updated.GetType().Name,
            };
        }

        public async Task DeleteUserAsync(int id)
        {
            var exists = await _userRepository.ExistsAsync(id);
            if (!exists)
            {
                throw new NotFoundException($"User with id {id} was not found.");
            }

            await _userRepository.DeleteAsync(id);
        }

        public async Task<UserDTO> AuthenticateAsync(LoginDTO dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || user.IsDeleted)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            return new UserDTO
            {
                Id = user.Id,
                RoleName = user.GetType().Name
            };
        }
    }
}
