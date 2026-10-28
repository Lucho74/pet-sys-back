using Application.Exceptions;
using Application.Interfaces;
using Application.Models;
using Domain.Constants;
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
        public readonly IPetRepository _petRepository;
        public readonly IConsultationRepository _consultationRepository;

        public UserServices(IUserRepository userRepository, IPetRepository petRepository, IConsultationRepository consultationRepository)
        {
            _userRepository = userRepository;
            _petRepository = petRepository;
            _consultationRepository = consultationRepository;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUserAsync()
        {
            IEnumerable<User> users = await _userRepository.GetAllAsync();

            return users.Select(user => new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsDeleted = user.IsDeleted,
                Roles = user.UserRoles
                .Select(ur => ur.Role.Name)
                .OrderBy(name => name)
                .ToArray(),
                Dni = user.ClientProfile?.Dni
            }).ToArray();
        }

        public async Task<UserDTO> GetUserByIdAsync(int id)
        {
            User? user = await _userRepository.GetByIdAsync(id);

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
                Roles = user.UserRoles
                .Select(ur => ur.Role.Name)
                .OrderBy(name => name)
                .ToArray(),
                Dni = user.ClientProfile?.Dni
            };
        }

        public async Task<UserDTO> AddUserAsync(CreateUserDTO dto)
        {
            User? sameEmail = await _userRepository.GetByEmailAsync(dto.Email);

            if (sameEmail != null)
            {
                throw new BadRequestException($"A user with email {dto.Email} already exists.");
            }

            if (string.IsNullOrWhiteSpace(dto.Dni) || dto.Dni.Trim().Length > 8)
            {
                throw new BadRequestException(
                    "Dni is required and must contain at most 8 characters.");
            }

            Role? clientRole = await _userRepository.GetRoleByNameAsync(RoleNames.Client);

            if (clientRole == null)
            {
                throw new InvalidOperationException(
                    "The Client role has not been seeded.");
            }

            User newUser = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                ClientProfile = new ClientProfile
                {
                    Dni = dto.Dni.Trim()
                }
            };

            newUser.UserRoles.Add(new UserRole
            {
                RoleId = clientRole.Id,
                Role = clientRole
            });

            User created = await _userRepository.AddAsync(newUser);

            return new UserDTO
            {
                Id = created.Id,
                FullName = created.FullName,
                Email = created.Email,
                Phone = created.Phone,
                IsDeleted = created.IsDeleted,
                Roles = created.UserRoles
                .Select(ur => ur.Role.Name)
                .OrderBy(name => name)
                .ToArray(),
                Dni = created.ClientProfile?.Dni
            };
        }

        public async Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO dto)
        {
            User? existing = await _userRepository.GetByIdAsync(id);

            if (existing == null)
            {
                throw new NotFoundException($"User with id {id} was not found.");
            }

            User? sameEmail = await _userRepository.GetByEmailAsync(dto.Email);

            if (sameEmail != null && sameEmail.Id != id)
            {
                throw new BadRequestException($"A user with email {dto.Email} already exists.");
            }

            if (dto.Dni != null)
            {
                if (existing.ClientProfile == null)
                {
                    throw new BadRequestException(
                        "The user does not have a client profile.");
                }

                if (string.IsNullOrWhiteSpace(dto.Dni) ||
                    dto.Dni.Trim().Length > 8)
                {
                    throw new BadRequestException(
                        "Dni must contain between 1 and 8 characters.");
                }

                existing.ClientProfile.Dni = dto.Dni.Trim();
            }

            existing.FullName = dto.FullName;
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                existing.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            User? updated = await _userRepository.UpdateAsync(id, existing);
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
                Roles = updated.UserRoles
                .Select(ur => ur.Role.Name)
                .OrderBy(name => name)
                .ToArray(),
                Dni = updated.ClientProfile?.Dni
            };
        }

        public async Task DeleteUserAsync(int id)
        {
            await SetDeletedAsync(id, true);
        }

        public async Task ReactivateUserAsync(int id)
        {
            await SetDeletedAsync(id, false);
        }

        private async Task SetDeletedAsync(int id, bool isDeleted)
        {
            User? existing = await _userRepository.GetByIdAsync(id);

            if (existing == null)
            {
                throw new NotFoundException(
                    $"User with id {id} was not found.");
            }

            foreach (var pet in await _petRepository.GetByClientIdAsync(id))
            {
                foreach (var consultation in
                    await _consultationRepository.GetByPetIdAsync(pet.Id))
                {
                    consultation.IsDeleted = isDeleted;

                    await _consultationRepository.UpdateAsync(
                        consultation.Id, consultation);
                }

                pet.IsDeleted = isDeleted;

                await _petRepository.UpdateAsync(pet.Id, pet);
            }

            existing.IsDeleted = isDeleted;

            await _userRepository.UpdateAsync(id, existing);
        }

        public async Task<UserDTO> AuthenticateAsync(LoginDTO dto)
        {
            User? user = await _userRepository.GetByEmailAsync(dto.Email);

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
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsDeleted = user.IsDeleted,
                Roles = user.UserRoles
                    .Select(ur => ur.Role.Name)
                    .OrderBy(name => name)
                    .ToArray(),
                Dni = user.ClientProfile?.Dni
            };
        }
    }
}
