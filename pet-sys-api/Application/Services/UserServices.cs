using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserServices
    {
        public readonly IUserRepository _userRepository;

        public UserServices(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDTO>?> GetAllUserAsync()
        {
            IEnumerable<User> user = await _userRepository.GetAllAsync();
            return user.Select(u => new UserDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                IsDeleted = u.IsDeleted
            });
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            var u = await _userRepository.GetByIdAsync(id);
            if (u == null) return null;
            return new UserDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                IsDeleted = u.IsDeleted
            };
        }

        public async Task<UserDTO?> AddUserAsync(CreateUserDTO dto)
        {
            var user = new Domain.Entities.Client
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Password = dto.Password ?? string.Empty 
            };

            var created = await _userRepository.AddAsync(user);
            return new UserDTO
            {
                Id = created.Id,
                FullName = created.FullName,
                Email = created.Email,
                Phone = created.Phone,
                IsDeleted = created.IsDeleted
            };
        }

        public async Task<UserDTO?> UpdateUserAsync(int id, UserDTO dto)
        {
            var existing = await _userRepository.GetByIdAsync(id);
            if (existing == null) return null;

            existing.FullName = dto.FullName;
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                existing.Password = dto.Password;
            }

            var updated = await _userRepository.UpdateAsync(id, existing);
            if (updated == null) return null;
            return new UserDTO
            {
                Id = updated.Id,
                FullName = updated.FullName,
                Email = updated.Email,
                Phone = updated.Phone,
                IsDeleted = updated.IsDeleted
            };
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var exists = await _userRepository.ExistsAsync(id);
            if (!exists) return false;
            await _userRepository.DeleteAsync(id);
            return true;
        }
    }
}
