using Application.Interfaces;
using Application.Models;
using Domain.Common;
using Application.Validation;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Enums;

namespace Application.Services
{
    public class UserServices : IUserServices
    {
        public readonly IUserRepository _userRepository;

        public UserServices(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDTO> GetUserByIdAsync(int id)
        {
            User? user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {id} was not found.");
            }

            return new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsSuspended = user.IsSuspended,
                RoleName = user.Role,
                Dni = user.ClientProfile?.Dni,
                RegistrationNumber = user.VeterinarianProfile?.RegistrationNumber
            };
        }

        public async Task<UserDTO> AddUserAsync(CreateUserDTO dto)
        {
            var role = UserRoleInputValidation.Validate(dto.Role);

            UserProfileRules.ValidateCreation(role, dto.Dni, dto.RegistrationNumber);

            User? sameEmail = await _userRepository.GetByEmailAsync(dto.Email);

            if (sameEmail != null)
            {
                throw new ArgumentException($"A user with email {dto.Email} already exists.");
            }

            User newUser = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = role
            };

            if (role == UserRole.Client)
            {
                newUser.ClientProfile = new ClientProfile
                {
                    Dni = dto.Dni!.Trim()
                };
            }

            if (role == UserRole.Veterinarian || role == UserRole.Admin)
            {
                newUser.VeterinarianProfile = new VeterinarianProfile
                {
                    RegistrationNumber = dto.RegistrationNumber!.Trim()
                };
            }

            User created = await _userRepository.AddAsync(newUser);

            return new UserDTO
            {
                Id = created.Id,
                FullName = created.FullName,
                Email = created.Email,
                Phone = created.Phone,
                IsSuspended = created.IsSuspended,
                RoleName = created.Role,
                Dni = created.ClientProfile?.Dni,
                RegistrationNumber = created.VeterinarianProfile?.RegistrationNumber
            };
        }

        public async Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO dto)
        {
            User? existing = await _userRepository.GetByIdAsync(id);

            if (existing == null)
            {
                throw new KeyNotFoundException($"User with id {id} was not found.");
            }

            if (existing.IsSuspended)
            {
                throw new ArgumentException($"User with id {id} is suspended.");
            }

            User? sameEmail = await _userRepository.GetByEmailAsync(dto.Email);

            if (sameEmail != null && sameEmail.Id != id)
            {
                throw new ArgumentException($"A user with email {dto.Email} already exists.");
            }

            UserProfileRules.ValidateRegistrationNumberUpdate(existing, dto.RegistrationNumber);

            if (dto.Dni != null)
            {
                if (existing.ClientProfile == null)
                {
                    throw new ArgumentException("The user does not have a client profile.");
                }

                if (string.IsNullOrWhiteSpace(dto.Dni) ||
                    dto.Dni.Trim().Length > 8)
                {
                    throw new ArgumentException("Dni must contain between 1 and 8 characters.");
                }

                existing.ClientProfile.Dni = dto.Dni.Trim();
            }

            existing.FullName = dto.FullName;
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;

            if (dto.RegistrationNumber != null)
            {
                existing.VeterinarianProfile!.RegistrationNumber = dto.RegistrationNumber.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                existing.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _userRepository.UpdateAsync(id, existing);

            return new UserDTO
            {
                Id = existing.Id,
                FullName = existing.FullName,
                Email = existing.Email,
                Phone = existing.Phone,
                IsSuspended = existing.IsSuspended,
                RoleName = existing.Role,
                Dni = existing.ClientProfile?.Dni,
                RegistrationNumber = existing.VeterinarianProfile?.RegistrationNumber
            };
        }

        public async Task UpdateUserRoleAsync(int id, UpdateUserRoleDTO dto)
        {
            User? existing = await _userRepository.GetByIdAsync(id);

            if (existing == null)
            {
                throw new KeyNotFoundException($"User with id {id} was not found.");
            }

            if (existing.Role == UserRole.SysAdmin)
            {
                throw new UnauthorizedAccessException("The SysAdmin account roles cannot be modified.");
            }

            var role = UserRoleInputValidation.Validate(dto.Role);

            UserProfileRules.ValidateRoleTransition(existing, role);

            existing.Role = role;

            await _userRepository.UpdateAsync(id, existing);
        }

        public async Task DeleteUserAsync(int id)
        {
            await SetSuspendedAsync(id, true);
        }

        public async Task ReactivateUserAsync(int id)
        {
            await SetSuspendedAsync(id, false);
        }

        private async Task SetSuspendedAsync(int id, bool isSuspended)
        {
            User? existing = await _userRepository.GetByIdAsync(id);

            if (existing == null)
            {
                throw new KeyNotFoundException($"User with id {id} was not found.");
            }

            if (existing.Role == UserRole.SysAdmin)
            {
                throw new UnauthorizedAccessException("The SysAdmin account status cannot be modified.");
            }

            await _userRepository.SetSuspendedAsync(id, isSuspended);
        }

        public async Task<UserDTO?> AuthenticateAsync(LoginDTO dto)
        {
            User? user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null || user.IsSuspended || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return null;
            }

            return new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsSuspended = user.IsSuspended,
                RoleName = user.Role,
                Dni = user.ClientProfile?.Dni,
                RegistrationNumber = user.VeterinarianProfile?.RegistrationNumber
            };
        }

        public async Task<PaginatedList<UserDTO>> GetUsersPagedAsync(UserQuery q, IReadOnlyCollection<UserRole> readableRoles)
        {
            if (q.PageIndex < 1 || q.PageSize < 1)
            {
                throw new ArgumentException("Page index and page size must be greater than 0.");
            }

            if (q.PageSize > 50)
            {
                throw new ArgumentException("Page size cannot be greater than 50.");
            }

            var page = await _userRepository.GetPagedAsync(q, readableRoles);

            var items = page.Items.Select(user => new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsSuspended = user.IsSuspended,
                RoleName = user.Role,
                Dni = user.ClientProfile?.Dni,
                RegistrationNumber = user.VeterinarianProfile?.RegistrationNumber
            }).ToList();

            return new PaginatedList<UserDTO>(items, page.PageIndex, page.TotalPages);
        }
    }
}
