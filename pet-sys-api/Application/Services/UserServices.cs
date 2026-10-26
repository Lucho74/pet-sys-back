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
        public readonly IPetRepository _petRepository;
        public readonly IConsultationRepository _consultationRepository;

        public UserServices(IUserRepository userRepository, IPetRepository petRepository, IConsultationRepository consultationRepository)
        {
            _userRepository = userRepository;
            _petRepository = petRepository;
            _consultationRepository = consultationRepository;
        }

        private static bool CanSee(User target, int callerId, string callerRole)
        {
            return target.Id == callerId || callerRole switch
            {
                "SysAdmin" => true,
                "Admin" => target is not SysAdmin,
                "Veterinarian" => target is Client,
                _ => false
            };
        }

        private static int Rank(string role) => role switch
        {
            "SysAdmin" => 3,
            "Admin" => 2,
            "Veterinarian" => 1,
            _ => 0
        };

        // Cada rol gestiona todos los roles que están por debajo; a SysAdmin no lo gestiona nadie.
        private static void EnsureCanManage(string targetRole, string callerRole)
        {
            if (Rank(callerRole) <= Rank(targetRole))
            {
                throw new ForbiddenException($"Role {callerRole} cannot manage users of role {targetRole}.");
            }
        }

        // 404 en vez de 403, para que un Admin no sepa que existe un SysAdmin.
        private async Task<User> GetVisibleUserAsync(int id, int callerId, string callerRole)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || !CanSee(user, callerId, callerRole))
            {
                throw new NotFoundException($"User with id {id} was not found.");
            }

            return user;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUserAsync(int callerId, string callerRole)
        {
            var users = await _userRepository.GetAllAsync();

            return users.Where(user => CanSee(user, callerId, callerRole)).Select(user => new UserDTO
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

        public async Task<UserDTO> GetUserByIdAsync(int id, int callerId, string callerRole)
        {
            var user = await GetVisibleUserAsync(id, callerId, callerRole);

            return new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsDeleted = user.IsDeleted,
                RoleName = user.GetType().Name,
                Dni = user is Client client ? client.Dni : null
            };
        }

        public async Task<UserDTO> AddUserAsync(CreateUserDTO dto, string callerRole)
        {
            EnsureCanManage(dto.UserType.ToString(), callerRole);

            if (await _userRepository.GetByEmailAsync(dto.Email) != null)
            {
                throw new BadRequestException($"A user with email {dto.Email} already exists.");
            }

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
                    if (string.IsNullOrWhiteSpace(dto.Dni))
                    {
                        throw new BadRequestException("Dni is required for clients.");
                    }

                    newUser = new Client
                    {
                        FullName = dto.FullName,
                        Email = dto.Email,
                        Phone = dto.Phone,
                        Password = dto.Password ?? string.Empty,
                        Dni = dto.Dni
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
                Dni = created is Client client ? client.Dni : null
            };
        }

        public async Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO dto, int callerId, string callerRole)
        {
            var existing = await GetVisibleUserAsync(id, callerId, callerRole);
            EnsureCanManage(existing.GetType().Name, callerRole);

            var sameEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (sameEmail != null && sameEmail.Id != id)
            {
                throw new BadRequestException($"A user with email {dto.Email} already exists.");
            }

            existing.FullName = dto.FullName;
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                existing.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }
            if (existing is Client existingClient && !string.IsNullOrWhiteSpace(dto.Dni))
            {
                existingClient.Dni = dto.Dni;
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
                Dni = updated is Client client ? client.Dni : null
            };
        }

        public async Task DeleteUserAsync(int id, int callerId, string callerRole)
        {
            await SetDeletedAsync(id, true, callerId, callerRole);
        }

        public async Task ReactivateUserAsync(int id, int callerId, string callerRole)
        {
            await SetDeletedAsync(id, false, callerId, callerRole);
        }

        // Los usuarios se suspenden, nunca se borran: borrar un Client elimina en cascada sus mascotas y consultas.
        private async Task SetDeletedAsync(int id, bool isDeleted, int callerId, string callerRole)
        {
            var existing = await GetVisibleUserAsync(id, callerId, callerRole);
            EnsureCanManage(existing.GetType().Name, callerRole);

            if (existing is Client)
            {
                foreach (var pet in await _petRepository.GetByClientIdAsync(id))
                {
                    foreach (var consultation in await _consultationRepository.GetByPetIdAsync(pet.Id))
                    {
                        consultation.IsDeleted = isDeleted;
                    }

                    pet.IsDeleted = isDeleted;
                }
            }

            // Los repositorios comparten el DbContext del request: este UpdateAsync guarda también las mascotas
            // y consultas modificadas arriba, todo en un solo SaveChanges (una transacción, todo o nada).
            existing.IsDeleted = isDeleted;
            await _userRepository.UpdateAsync(id, existing);
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
