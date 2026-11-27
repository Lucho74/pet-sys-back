using Application.Interfaces;
using Application.Models;
using Domain.Common;
using Application.Validation;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Enums;
using Application.Authorization;

namespace Application.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;

        public UserServices(IUserRepository userRepository, ICurrentUser currentUser)
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
        }

        public async Task<UserDTO> GetCurrentUserAsync()
        {
            var callerId = _currentUser.Id;

            if (!callerId.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }
            User? user = await _userRepository.GetByIdAsync(callerId.Value);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {callerId.Value} was not found.");
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

        public async Task<UserDTO> GetUserByIdAsync(int id)
        {
            var callerRole = _currentUser.Role;

            if (!callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            if (!AccessRules.IsStaff(callerRole.Value))
            {
                throw new UnauthorizedAccessException("Only staff can perform this operation.");
            }
            User? user = await _userRepository.GetByIdAsync(id);

            if (user == null || !AccessRules.CanManageAccount(callerRole.Value, user.Role))
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
            var callerRole = _currentUser.Role;

            if (!callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            if (!AccessRules.IsStaff(callerRole.Value))
            {
                throw new UnauthorizedAccessException("Only staff can perform this operation.");
            }

            if (dto.Role.HasValue && !AccessRules.CanManageAccount(callerRole.Value, dto.Role.Value))
            {
                throw new UnauthorizedAccessException("You cannot create an account with the requested role.");
            }

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
            var callerId = _currentUser.Id;
            var callerRole = _currentUser.Role;

            if (!callerId.HasValue || !callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            if (!AccessRules.IsStaff(callerRole.Value))
            {
                throw new UnauthorizedAccessException("Only staff can perform this operation.");
            }
            User? existing = await _userRepository.GetByIdAsync(id);

            if (existing == null || !AccessRules.CanUpdateAccount(callerId.Value, callerRole.Value, existing.Id, existing.Role))
            {
                throw new KeyNotFoundException($"User with id {id} was not found.");
            }

            if (dto.RegistrationNumber != null && !AccessRules.CanUpdateRegistrationNumber(callerRole.Value))
            {
                throw new UnauthorizedAccessException("You cannot update a registration number.");
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
            var callerRole = _currentUser.Role;

            if (!callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            if (!AccessRules.CanChangeUserRole(callerRole.Value))
            {
                throw new UnauthorizedAccessException("Only SysAdmin can change user roles.");
            }

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
            var callerRole = _currentUser.Role;

            if (!callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            if (!AccessRules.IsStaff(callerRole.Value))
            {
                throw new UnauthorizedAccessException("Only staff can perform this operation.");
            }
            User? existing = await _userRepository.GetByIdAsync(id);

            if (existing == null || !AccessRules.CanManageAccount(callerRole.Value, existing.Role))
            {
                throw new KeyNotFoundException($"User with id {id} was not found.");
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

        public async Task<PaginatedList<UserDTO>> GetUsersPagedAsync(UserQuery q)
        {
            var callerRole = _currentUser.Role;

            if (!callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            if (!AccessRules.IsStaff(callerRole.Value))
            {
                throw new UnauthorizedAccessException("Only staff can perform this operation.");
            }
            var readableRoles = AccessRules.GetManageableRoles(callerRole.Value);

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
