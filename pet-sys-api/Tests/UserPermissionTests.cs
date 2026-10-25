using Application.Exceptions;
using Application.Models;
using Application.Services;
using Domain.Entities;

namespace Tests
{
    [Trait("Category", "Unit")]
    public class UserPermissionTests
    {
        private const int SysAdminId = 1, AdminId = 2, VetId = 3, OtherVetId = 4, ClientId = 5;

        private readonly FakeUserRepository _userRepository = new();
        private readonly UserServices _sut;

        public UserPermissionTests()
        {
            _userRepository.Items.AddRange(new User[]
            {
                new SysAdmin { Id = SysAdminId, FullName = "Sys", Email = "sys@test.com", Phone = "1", Password = "x" },
                new Admin { Id = AdminId, FullName = "Admin", Email = "admin@test.com", Phone = "1", Password = "x" },
                new Veterinarian { Id = VetId, FullName = "Vet", Email = "vet@test.com", Phone = "1", Password = "x" },
                new Veterinarian { Id = OtherVetId, FullName = "Vet 2", Email = "vet2@test.com", Phone = "1", Password = "x" },
                new Client { Id = ClientId, FullName = "Client", Email = "client@test.com", Phone = "1", Password = "x", Dni = "123" }
            });
            _sut = new UserServices(_userRepository);
        }

        private static CreateUserDTO NewUser(UserType type) => new()
        {
            FullName = "New", Email = "new@test.com", Phone = "1", Password = "secret123", UserType = type, Dni = "999"
        };

        [Theory]
        [InlineData("Veterinarian", UserType.Client)]
        [InlineData("SysAdmin", UserType.Client)]
        [InlineData("Admin", UserType.Veterinarian)]
        [InlineData("SysAdmin", UserType.Veterinarian)]
        [InlineData("SysAdmin", UserType.Admin)]
        public async Task AddUser_WhenCallerManagesTargetRole_CreatesUser(string callerRole, UserType target)
        {
            // Act
            var created = await _sut.AddUserAsync(NewUser(target), callerRole);

            // Assert
            Assert.Equal(target.ToString(), created.RoleName);
        }

        [Theory]
        [InlineData("Admin", UserType.Client)]
        [InlineData("Veterinarian", UserType.Veterinarian)]
        [InlineData("Admin", UserType.Admin)]
        [InlineData("Veterinarian", UserType.Admin)]
        public async Task AddUser_WhenCallerDoesNotManageTargetRole_ThrowsForbidden(string callerRole, UserType target)
        {
            // Act
            var act = () => _sut.AddUserAsync(NewUser(target), callerRole);

            // Assert
            await Assert.ThrowsAsync<ForbiddenException>(act);
        }

        [Fact]
        public async Task AddUser_WithExistingEmail_ThrowsBadRequest()
        {
            // Arrange
            var dto = NewUser(UserType.Client);
            dto.Email = "client@test.com";

            // Act
            var act = () => _sut.AddUserAsync(dto, "SysAdmin");

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task GetUserById_WhenAdminRequestsSysAdmin_ThrowsNotFound()
        {
            // Act
            var act = () => _sut.GetUserByIdAsync(SysAdminId, AdminId, "Admin");

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task GetUserById_WhenVetRequestsSelf_ReturnsUser()
        {
            // Act
            var user = await _sut.GetUserByIdAsync(VetId, VetId, "Veterinarian");

            // Assert
            Assert.Equal(VetId, user.Id);
        }

        [Fact]
        public async Task GetUserById_WhenVetRequestsOtherVet_ThrowsNotFound()
        {
            // Act
            var act = () => _sut.GetUserByIdAsync(OtherVetId, VetId, "Veterinarian");

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task GetAllUser_WhenAdmin_ExcludesSysAdmins()
        {
            // Act
            var users = await _sut.GetAllUserAsync(AdminId, "Admin");

            // Assert
            Assert.DoesNotContain(users, u => u.RoleName == "SysAdmin");
        }

        [Fact]
        public async Task GetAllUser_WhenVet_ReturnsOnlyClientsAndSelf()
        {
            // Act
            var users = await _sut.GetAllUserAsync(VetId, "Veterinarian");

            // Assert
            Assert.Equal(new[] { VetId, ClientId }, users.Select(u => u.Id).OrderBy(id => id));
        }

        [Fact]
        public async Task DeleteUser_WhenAdminTargetsSysAdmin_ThrowsNotFound()
        {
            // Act
            var act = () => _sut.DeleteUserAsync(SysAdminId, AdminId, "Admin");

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task DeleteUser_WhenAllowed_SuspendsInsteadOfRemoving()
        {
            // Act
            await _sut.DeleteUserAsync(ClientId, VetId, "Veterinarian");

            // Assert
            var client = Assert.Single(_userRepository.Items, u => u.Id == ClientId);
            Assert.True(client.IsDeleted);
        }

        [Fact]
        public async Task ReactivateUser_WhenSuspended_ClearsIsDeleted()
        {
            // Arrange
            _userRepository.Items.Single(u => u.Id == ClientId).IsDeleted = true;

            // Act
            await _sut.ReactivateUserAsync(ClientId, VetId, "Veterinarian");

            // Assert
            Assert.False(_userRepository.Items.Single(u => u.Id == ClientId).IsDeleted);
        }
    }
}
