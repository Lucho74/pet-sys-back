using Application.Exceptions;
using Application.Interfaces;
using Application.Models;
using Domain.Common;
using Application.Validation;
using Domain.Constants;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class UserServices : IUserServices
    {
        public readonly IUserRepository _userRepository;

        public UserServices(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUserAsync(IReadOnlyCollection<string> readableRoles)
        {
            var allowedRoles = readableRoles.ToArray();

            IEnumerable<User> users = await _userRepository.FindAsync(user => allowedRoles.Contains(user.Role.Name));

            return users.Select(user => new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsSuspended = user.IsSuspended,
                RoleName = user.Role.Name,
                Dni = user.ClientProfile?.Dni,
                RegistrationNumber = user.VeterinarianProfile?.RegistrationNumber
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
                IsSuspended = user.IsSuspended,
                RoleName = user.Role.Name,
                Dni = user.ClientProfile?.Dni,
                RegistrationNumber = user.VeterinarianProfile?.RegistrationNumber
            };
        }

        public async Task<UserDTO> AddUserAsync(CreateUserDTO dto)
        {
            UserRoleInputValidation.Validate(dto.Role);

            UserProfileRules.ValidateCreation(dto.Role, dto.Dni, dto.RegistrationNumber);

            User? sameEmail = await _userRepository.GetByEmailAsync(dto.Email);

            if (sameEmail != null)
            {
                throw new BadRequestException($"A user with email {dto.Email} already exists.");
            }

            Role? role = await _userRepository.GetRoleByNameAsync(dto.Role);

            if (role == null)
            {
                throw new InvalidOperationException($"The role {dto.Role} has not been seeded.");
            }

            User newUser = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = role.Id,
                Role = role
            };

            if (dto.Role == RoleNames.Client)
            {
                newUser.ClientProfile = new ClientProfile
                {
                    Dni = dto.Dni!.Trim()
                };
            }

            if (dto.Role == RoleNames.Veterinarian || dto.Role == RoleNames.Admin)
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
                RoleName = created.Role.Name,
                Dni = created.ClientProfile?.Dni,
                RegistrationNumber = created.VeterinarianProfile?.RegistrationNumber
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

            UserProfileRules.ValidateRegistrationNumberUpdate(existing, dto.RegistrationNumber);

            if (dto.Dni != null)
            {
                if (existing.ClientProfile == null)
                {
                    throw new BadRequestException("The user does not have a client profile.");
                }

                if (string.IsNullOrWhiteSpace(dto.Dni) ||
                    dto.Dni.Trim().Length > 8)
                {
                    throw new BadRequestException("Dni must contain between 1 and 8 characters.");
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
                IsSuspended = updated.IsSuspended,
                RoleName = updated.Role.Name,
                Dni = updated.ClientProfile?.Dni,
                RegistrationNumber = updated.VeterinarianProfile?.RegistrationNumber
            };
        }

        public async Task UpdateUserRoleAsync(int id, UpdateUserRoleDTO dto)
        {
            User? existing = await _userRepository.GetByIdAsync(id);

            if (existing == null)
            {
                throw new NotFoundException( $"User with id {id} was not found.");
            }

            if (existing.Role.Name == RoleNames.SysAdmin)
            {
                throw new ForbiddenException("The SysAdmin account roles cannot be modified.");
            }

            UserRoleInputValidation.Validate(dto.Role);

            UserProfileRules.ValidateRoleTransition(existing, dto.Role);

            Role? role = await _userRepository.GetRoleByNameAsync(dto.Role);

            if (role == null)
            {
                throw new InvalidOperationException($"The role {dto.Role} has not been seeded.");
            }

            existing.RoleId = role.Id;
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
                throw new NotFoundException($"User with id {id} was not found.");
            }

            if (existing.Role.Name == RoleNames.SysAdmin)
            {
                throw new ForbiddenException("The SysAdmin account status cannot be modified.");
            }

            await _userRepository.SetSuspendedAsync(id, isSuspended);
        }

        public async Task<UserDTO> AuthenticateAsync(LoginDTO dto)
        {
            User? user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null || user.IsSuspended)
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
                IsSuspended = user.IsSuspended,
                RoleName = user.Role.Name,
                Dni = user.ClientProfile?.Dni,
                RegistrationNumber = user.VeterinarianProfile?.RegistrationNumber
            };
        }

        public async Task<PaginatedList<UserDTO>> GetUsersPagedAsync(UserQuery q, IReadOnlyCollection<string> readableRoles)
        {
            if (q.PageIndex < 1 || q.PageSize < 1)
            {
                throw new BadRequestException("Page index and page size must be greater than 0.");
            }

            if (q.PageSize > 50)
            {
                throw new BadRequestException("Page size cannot be greater than 50.");
            }

            var page = await _userRepository.GetPagedAsync(q, readableRoles);

            var items = page.Items.Select(user => new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsSuspended = user.IsSuspended,
                RoleName = user.Role.Name,
                Dni = user.ClientProfile?.Dni,
                RegistrationNumber = user.VeterinarianProfile?.RegistrationNumber
            }).ToList();

            return new PaginatedList<UserDTO>(items, page.PageIndex, page.TotalPages);
        }
    }
}
