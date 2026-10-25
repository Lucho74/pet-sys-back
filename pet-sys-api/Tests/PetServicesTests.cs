using Application.Exceptions;
using Application.Models;
using Application.Services;
using Domain.Entities;

namespace Tests
{
    [Trait("Category", "Unit")]
    public class PetServicesTests
    {
        private const int ClientId = 1, OtherClientId = 2, VetId = 3;

        private readonly FakePetRepository _petRepository = new();
        private readonly FakeUserRepository _userRepository = new();
        private readonly PetServices _sut;

        public PetServicesTests()
        {
            _userRepository.Items.AddRange(new User[]
            {
                new Client { Id = ClientId, FullName = "Client", Email = "c1@test.com", Phone = "1", Password = "x", Dni = "1" },
                new Client { Id = OtherClientId, FullName = "Client 2", Email = "c2@test.com", Phone = "1", Password = "x", Dni = "2" },
                new Veterinarian { Id = VetId, FullName = "Vet", Email = "vet@test.com", Phone = "1", Password = "x" }
            });
            _sut = new PetServices(_petRepository, _userRepository);
        }

        private static Pet NewPet(int id, int clientId) => new()
        {
            Id = id, Name = "Pet " + id, Specie = "Dog", Breed = "Mixed", BirthDate = new DateOnly(2020, 1, 1), ClientId = clientId
        };

        private static CreatePetDTO NewPetDto(int clientId) => new()
        {
            Name = "Firulais", Specie = "Dog", Breed = "Mixed", BirthDate = new DateOnly(2021, 5, 10), ClientId = clientId
        };

        [Fact]
        public async Task GetAllPet_WhenNoPets_ReturnsEmptyList()
        {
            // Act
            var pets = await _sut.GetAllPetAsync();

            // Assert
            Assert.Empty(pets);
        }

        [Fact]
        public async Task GetPetById_WhenMissing_ThrowsNotFound()
        {
            // Act
            var act = () => _sut.GetPetByIdAsync(99);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task AddPet_WithExistingClient_CreatesPet()
        {
            // Act
            var created = await _sut.AddPetAsync(NewPetDto(ClientId));

            // Assert
            var stored = Assert.Single(_petRepository.Items);
            Assert.Equal(stored.Id, created.Id);
            Assert.Equal("Firulais", stored.Name);
        }

        [Theory]
        [InlineData(99)]
        [InlineData(VetId)]
        public async Task AddPet_WhenClientIdIsNotAClient_ThrowsBadRequest(int clientId)
        {
            // Act
            var act = () => _sut.AddPetAsync(NewPetDto(clientId));

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task UpdatePet_WhenMissing_ThrowsNotFound()
        {
            // Arrange
            var dto = new PetDTO { Name = "X", Specie = "Dog", Breed = "Mixed", BirthDate = new DateOnly(2020, 1, 1), ClientId = ClientId };

            // Act
            var act = () => _sut.UpdatePetAsync(99, dto);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task UpdatePet_WithValidData_ChangesStoredPet()
        {
            // Arrange
            _petRepository.Items.Add(NewPet(1, ClientId));
            var dto = new PetDTO { Name = "Renamed", Specie = "Cat", Breed = "Siamese", BirthDate = new DateOnly(2019, 3, 3), ClientId = OtherClientId };

            // Act
            var updated = await _sut.UpdatePetAsync(1, dto);

            // Assert
            Assert.Equal("Renamed", updated.Name);
            Assert.Equal(OtherClientId, _petRepository.Items.Single().ClientId);
        }

        [Fact]
        public async Task UpdatePet_WhenClientIdIsNotAClient_ThrowsBadRequest()
        {
            // Arrange
            _petRepository.Items.Add(NewPet(1, ClientId));
            var dto = new PetDTO { Name = "X", Specie = "Dog", Breed = "Mixed", BirthDate = new DateOnly(2020, 1, 1), ClientId = VetId };

            // Act
            var act = () => _sut.UpdatePetAsync(1, dto);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task DeletePet_WhenExists_RemovesPet()
        {
            // Arrange
            _petRepository.Items.Add(NewPet(1, ClientId));

            // Act
            await _sut.DeletePetAsync(1);

            // Assert
            Assert.Empty(_petRepository.Items);
        }

        [Fact]
        public async Task DeletePet_WhenMissing_ThrowsNotFound()
        {
            // Act
            var act = () => _sut.DeletePetAsync(99);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task GetPetsByClient_ReturnsOnlyThatClientsPets()
        {
            // Arrange
            _petRepository.Items.AddRange(new[] { NewPet(1, ClientId), NewPet(2, OtherClientId), NewPet(3, ClientId) });

            // Act
            var pets = await _sut.GetPetsByClientAsync(ClientId);

            // Assert
            Assert.Equal(new[] { 1, 3 }, pets.Select(p => p.Id));
        }

        [Fact]
        public async Task GetPetsByClient_WhenClientHasNoPets_ReturnsEmptyList()
        {
            // Act
            var pets = await _sut.GetPetsByClientAsync(ClientId);

            // Assert
            Assert.Empty(pets);
        }
    }
}
