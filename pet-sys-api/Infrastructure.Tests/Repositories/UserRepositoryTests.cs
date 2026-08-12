using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Tests.TestHelpers;

namespace Infrastructure.Tests.Repositories
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
        {
            using var context = InMemoryDbContextFactory.Create();
            var repository = new UserRepository(context);
            var client = EntityFactory.CreateClient(email: "found@test.com");
            await repository.AddAsync(client);

            var found = await repository.GetByEmailAsync("found@test.com");

            Assert.NotNull(found);
            Assert.Equal(client.Id, found!.Id);
        }

        [Fact]
        public async Task GetByEmailAsync_MissingEmail_ReturnsNull()
        {
            using var context = InMemoryDbContextFactory.Create();
            var repository = new UserRepository(context);

            var found = await repository.GetByEmailAsync("missing@test.com");

            Assert.Null(found);
        }

        [Fact]
        public async Task GetByRoleAsync_ReturnsOnlyUsersOfThatRole()
        {
            using var context = InMemoryDbContextFactory.Create();
            var repository = new UserRepository(context);
            var client = EntityFactory.CreateClient(email: "client@test.com");
            var vet = EntityFactory.CreateVeterinarian(email: "vet@test.com");
            var admin = EntityFactory.CreateAdmin(email: "admin@test.com");
            await repository.AddAsync(client);
            await repository.AddAsync(vet);
            await repository.AddAsync(admin);

            var clients = await repository.GetByRoleAsync<Client>();

            var match = Assert.Single(clients);
            Assert.Equal(client.Id, match.Id);
        }
    }
}
