using Domain.Entities;
using Infrastructure.Repositories;

namespace Infrastructure.Tests.Repositories
{
    public class BaseRepositoryTests
    {
        private static Pet CreatePet(int id, string name = "Firulais") => new()
        {
            Id = id,
            Name = name,
            Specie = "Dog",
            Breed = "Labrador",
            BirthDate = new DateOnly(2020, 1, 1),
            ClientId = 1,
        };

        [Fact]
        public async Task AddAsync_ReturnsTheSameEntity()
        {
            var repository = new BaseRepository<Pet>();
            var pet = CreatePet(1);

            var added = await repository.AddAsync(pet);

            Assert.Same(pet, added);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsEntity()
        {
            var repository = new BaseRepository<Pet>();
            var pet = CreatePet(1);
            await repository.AddAsync(pet);

            var found = await repository.GetByIdAsync(1);

            Assert.Same(pet, found);
        }

        [Fact]
        public async Task GetByIdAsync_MissingId_ThrowsKeyNotFoundException()
        {
            var repository = new BaseRepository<Pet>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.GetByIdAsync(999));
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEveryAddedEntity()
        {
            var repository = new BaseRepository<Pet>();
            await repository.AddAsync(CreatePet(1));
            await repository.AddAsync(CreatePet(2));

            var all = await repository.GetAllAsync();

            Assert.Equal(2, all.Count());
        }

        [Fact]
        public async Task FindAsync_ReturnsOnlyEntitiesMatchingPredicate()
        {
            var repository = new BaseRepository<Pet>();
            await repository.AddAsync(CreatePet(1, "Firulais"));
            await repository.AddAsync(CreatePet(2, "Michi"));

            var found = await repository.FindAsync(p => p.Name == "Michi");

            var match = Assert.Single(found);
            Assert.Equal(2, match.Id);
        }

        [Fact]
        public async Task UpdateAsync_ExistingId_ReplacesStoredEntity()
        {
            var repository = new BaseRepository<Pet>();
            await repository.AddAsync(CreatePet(1, "Firulais"));

            await repository.UpdateAsync(CreatePet(1, "Renamed"));

            var found = await repository.GetByIdAsync(1);
            Assert.Equal("Renamed", found.Name);
        }

        [Fact]
        public async Task UpdateAsync_MissingId_DoesNotAddOrThrow()
        {
            var repository = new BaseRepository<Pet>();

            await repository.UpdateAsync(CreatePet(999));

            var all = await repository.GetAllAsync();
            Assert.Empty(all);
        }

        [Fact]
        public async Task DeleteAsync_ExistingId_RemovesEntity()
        {
            var repository = new BaseRepository<Pet>();
            await repository.AddAsync(CreatePet(1));

            await repository.DeleteAsync(1);

            Assert.False(await repository.ExistsAsync(1));
        }

        [Fact]
        public async Task DeleteAsync_MissingId_DoesNotThrow()
        {
            var repository = new BaseRepository<Pet>();

            var exception = await Record.ExceptionAsync(() => repository.DeleteAsync(999));

            Assert.Null(exception);
        }

        [Fact]
        public async Task ExistsAsync_ExistingId_ReturnsTrue()
        {
            var repository = new BaseRepository<Pet>();
            await repository.AddAsync(CreatePet(1));

            Assert.True(await repository.ExistsAsync(1));
        }

        [Fact]
        public async Task ExistsAsync_MissingId_ReturnsFalse()
        {
            var repository = new BaseRepository<Pet>();

            Assert.False(await repository.ExistsAsync(999));
        }
    }
}
