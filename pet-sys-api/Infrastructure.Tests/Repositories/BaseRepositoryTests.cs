using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Tests.TestHelpers;

namespace Infrastructure.Tests.Repositories
{
    public class BaseRepositoryTests
    {
        [Fact]
        public async Task AddAsync_ReturnsTheSameEntityWithGeneratedId()
        {
            using var context = InMemoryDbContextFactory.Create();
            var client = EntityFactory.CreateClient();
            context.Clients.Add(client);
            await context.SaveChangesAsync();
            var repository = new BaseRepository<Pet>(context);
            var pet = EntityFactory.CreatePet(client.Id);

            var added = await repository.AddAsync(pet);

            Assert.Same(pet, added);
            Assert.NotEqual(0, added.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsEntity()
        {
            using var context = InMemoryDbContextFactory.Create();
            var client = EntityFactory.CreateClient();
            context.Clients.Add(client);
            await context.SaveChangesAsync();
            var repository = new BaseRepository<Pet>(context);
            var pet = await repository.AddAsync(EntityFactory.CreatePet(client.Id));

            var found = await repository.GetByIdAsync(pet.Id);

            Assert.NotNull(found);
            Assert.Equal(pet.Id, found!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_MissingId_ReturnsNull()
        {
            using var context = InMemoryDbContextFactory.Create();
            var repository = new BaseRepository<Pet>(context);

            var found = await repository.GetByIdAsync(999);

            Assert.Null(found);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEveryAddedEntity()
        {
            using var context = InMemoryDbContextFactory.Create();
            var client = EntityFactory.CreateClient();
            context.Clients.Add(client);
            await context.SaveChangesAsync();
            var repository = new BaseRepository<Pet>(context);
            await repository.AddAsync(EntityFactory.CreatePet(client.Id, "Firulais"));
            await repository.AddAsync(EntityFactory.CreatePet(client.Id, "Michi"));

            var all = await repository.GetAllAsync();

            Assert.Equal(2, all.Count());
        }

        [Fact]
        public async Task FindAsync_ReturnsOnlyEntitiesMatchingPredicate()
        {
            using var context = InMemoryDbContextFactory.Create();
            var client = EntityFactory.CreateClient();
            context.Clients.Add(client);
            await context.SaveChangesAsync();
            var repository = new BaseRepository<Pet>(context);
            await repository.AddAsync(EntityFactory.CreatePet(client.Id, "Firulais"));
            var michi = await repository.AddAsync(EntityFactory.CreatePet(client.Id, "Michi"));

            var found = await repository.FindAsync(p => p.Name == "Michi");

            var match = Assert.Single(found);
            Assert.Equal(michi.Id, match.Id);
        }

        [Fact]
        public async Task UpdateAsync_ExistingId_ReplacesStoredEntityAndReturnsIt()
        {
            using var context = InMemoryDbContextFactory.Create();
            var client = EntityFactory.CreateClient();
            context.Clients.Add(client);
            await context.SaveChangesAsync();
            var repository = new BaseRepository<Pet>(context);
            var pet = await repository.AddAsync(EntityFactory.CreatePet(client.Id, "Firulais"));
            var replacement = EntityFactory.CreatePet(client.Id, "Renamed");
            replacement.Id = pet.Id;

            var updated = await repository.UpdateAsync(pet.Id, replacement);

            Assert.NotNull(updated);
            Assert.Equal("Renamed", updated!.Name);
            var found = await repository.GetByIdAsync(pet.Id);
            Assert.Equal("Renamed", found!.Name);
        }

        [Fact]
        public async Task UpdateAsync_MissingId_ReturnsNull()
        {
            using var context = InMemoryDbContextFactory.Create();
            var client = EntityFactory.CreateClient();
            context.Clients.Add(client);
            await context.SaveChangesAsync();
            var repository = new BaseRepository<Pet>(context);

            var updated = await repository.UpdateAsync(999, EntityFactory.CreatePet(client.Id));

            Assert.Null(updated);
        }

        [Fact]
        public async Task DeleteAsync_ExistingId_RemovesEntity()
        {
            using var context = InMemoryDbContextFactory.Create();
            var client = EntityFactory.CreateClient();
            context.Clients.Add(client);
            await context.SaveChangesAsync();
            var repository = new BaseRepository<Pet>(context);
            var pet = await repository.AddAsync(EntityFactory.CreatePet(client.Id));

            await repository.DeleteAsync(pet.Id);

            Assert.False(await repository.ExistsAsync(pet.Id));
        }

        [Fact]
        public async Task DeleteAsync_MissingId_DoesNotThrow()
        {
            using var context = InMemoryDbContextFactory.Create();
            var repository = new BaseRepository<Pet>(context);

            var exception = await Record.ExceptionAsync(() => repository.DeleteAsync(999));

            Assert.Null(exception);
        }

        [Fact]
        public async Task ExistsAsync_ExistingId_ReturnsTrue()
        {
            using var context = InMemoryDbContextFactory.Create();
            var client = EntityFactory.CreateClient();
            context.Clients.Add(client);
            await context.SaveChangesAsync();
            var repository = new BaseRepository<Pet>(context);
            var pet = await repository.AddAsync(EntityFactory.CreatePet(client.Id));

            Assert.True(await repository.ExistsAsync(pet.Id));
        }

        [Fact]
        public async Task ExistsAsync_MissingId_ReturnsFalse()
        {
            using var context = InMemoryDbContextFactory.Create();
            var repository = new BaseRepository<Pet>(context);

            Assert.False(await repository.ExistsAsync(999));
        }
    }
}
