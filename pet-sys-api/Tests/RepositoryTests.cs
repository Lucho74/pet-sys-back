using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    // InMemory enforces neither foreign keys nor the unique email index; those still need SQL Server.
    [Trait("Category", "Integration")]
    public class RepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public RepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
        }

        public void Dispose() => _context.Dispose();

        private static Pet NewPet(int clientId, string name = "Pet") => new()
        {
            Name = name, Specie = "Dog", Breed = "Mixed", BirthDate = new DateOnly(2020, 1, 1), ClientId = clientId
        };

        private static Consultation NewConsultation(int petId, int veterinarianId) => new()
        {
            Description = "Checkup", Date = new DateTime(2026, 10, 1, 10, 0, 0), PetId = petId, VeterinarianId = veterinarianId
        };

        [Fact]
        public async Task AddAsync_AssignsIdAndPersists()
        {
            // Arrange
            var sut = new PetRepository(_context);

            // Act
            var created = await sut.AddAsync(NewPet(1));

            // Assert
            Assert.True(created.Id > 0);
            Assert.True(await sut.ExistsAsync(created.Id));
        }

        [Fact]
        public async Task GetByIdAsync_WhenMissing_ReturnsNull()
        {
            // Arrange
            var sut = new PetRepository(_context);

            // Act
            var pet = await sut.GetByIdAsync(99);

            // Assert
            Assert.Null(pet);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEveryRow()
        {
            // Arrange
            var sut = new PetRepository(_context);
            await sut.AddAsync(NewPet(1));
            await sut.AddAsync(NewPet(2));

            // Act
            var pets = await sut.GetAllAsync();

            // Assert
            Assert.Equal(2, pets.Count());
        }

        [Fact]
        public async Task FindAsync_FiltersByPredicate()
        {
            // Arrange
            var sut = new PetRepository(_context);
            await sut.AddAsync(NewPet(1, "Firulais"));
            await sut.AddAsync(NewPet(1, "Michi"));

            // Act
            var pets = await sut.FindAsync(p => p.Name == "Michi");

            // Assert
            Assert.Equal("Michi", Assert.Single(pets).Name);
        }

        [Fact]
        public async Task UpdateAsync_WhenExists_SavesNewValues()
        {
            // Arrange
            var sut = new PetRepository(_context);
            var created = await sut.AddAsync(NewPet(1, "Old"));
            var changes = NewPet(1, "New");
            changes.Id = created.Id;

            // Act
            var updated = await sut.UpdateAsync(created.Id, changes);

            // Assert
            Assert.Equal("New", updated!.Name);
            Assert.Equal("New", (await sut.GetByIdAsync(created.Id))!.Name);
        }

        [Fact]
        public async Task UpdateAsync_WhenMissing_ReturnsNull()
        {
            // Arrange
            var sut = new PetRepository(_context);

            // Act
            var updated = await sut.UpdateAsync(99, NewPet(1));

            // Assert
            Assert.Null(updated);
        }

        [Fact]
        public async Task DeleteAsync_WhenExists_RemovesRow()
        {
            // Arrange
            var sut = new PetRepository(_context);
            var created = await sut.AddAsync(NewPet(1));

            // Act
            await sut.DeleteAsync(created.Id);

            // Assert
            Assert.False(await sut.ExistsAsync(created.Id));
        }

        [Fact]
        public async Task DeleteAsync_WhenMissing_DoesNothing()
        {
            // Arrange
            var sut = new PetRepository(_context);
            await sut.AddAsync(NewPet(1));

            // Act
            await sut.DeleteAsync(99);

            // Assert
            Assert.Single(await sut.GetAllAsync());
        }

        [Fact]
        public async Task PetRepository_GetByClientIdAsync_ReturnsOnlyThatClientsPets()
        {
            // Arrange
            var sut = new PetRepository(_context);
            await sut.AddAsync(NewPet(1, "Mine"));
            await sut.AddAsync(NewPet(2, "Theirs"));

            // Act
            var pets = await sut.GetByClientIdAsync(1);

            // Assert
            Assert.Equal("Mine", Assert.Single(pets).Name);
        }

        [Fact]
        public async Task ConsultationRepository_GetByPetIdAsync_ReturnsOnlyThatPetsConsultations()
        {
            // Arrange
            var sut = new ConsultationRepository(_context);
            await sut.AddAsync(NewConsultation(petId: 1, veterinarianId: 5));
            await sut.AddAsync(NewConsultation(petId: 2, veterinarianId: 5));

            // Act
            var consultations = await sut.GetByPetIdAsync(1);

            // Assert
            Assert.Equal(1, Assert.Single(consultations).PetId);
        }

        [Fact]
        public async Task ConsultationRepository_GetByVeterinarianIdAsync_ReturnsOnlyThatVetsConsultations()
        {
            // Arrange
            var sut = new ConsultationRepository(_context);
            await sut.AddAsync(NewConsultation(petId: 1, veterinarianId: 5));
            await sut.AddAsync(NewConsultation(petId: 1, veterinarianId: 6));

            // Act
            var consultations = await sut.GetByVeterinarianIdAsync(6);

            // Assert
            Assert.Equal(6, Assert.Single(consultations).VeterinarianId);
        }

        [Fact]
        public async Task UserRepository_GetByEmailAsync_FindsUserOfAnyRole()
        {
            // Arrange
            var sut = new UserRepository(_context);
            await sut.AddAsync(new Veterinarian { FullName = "Vet", Email = "vet@test.com", Phone = "1", Password = "x" });

            // Act
            var user = await sut.GetByEmailAsync("vet@test.com");

            // Assert
            Assert.IsType<Veterinarian>(user);
        }

        [Fact]
        public async Task UserRepository_GetByEmailAsync_WhenMissing_ReturnsNull()
        {
            // Arrange
            var sut = new UserRepository(_context);

            // Act
            var user = await sut.GetByEmailAsync("nobody@test.com");

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task UserRepository_GetByRoleAsync_ReturnsOnlyThatRole()
        {
            // Arrange
            var sut = new UserRepository(_context);
            await sut.AddAsync(new Veterinarian { FullName = "Vet", Email = "vet@test.com", Phone = "1", Password = "x" });
            await sut.AddAsync(new Client { FullName = "Client", Email = "c@test.com", Phone = "1", Password = "x", Dni = "1" });

            // Act
            var vets = await sut.GetByRoleAsync<Veterinarian>();

            // Assert
            Assert.Equal("Vet", Assert.Single(vets).FullName);
        }
    }
}
