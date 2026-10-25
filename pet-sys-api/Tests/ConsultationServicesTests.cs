using Application.Exceptions;
using Application.Models;
using Application.Services;
using Domain.Entities;

namespace Tests
{
    [Trait("Category", "Unit")]
    public class ConsultationServicesTests
    {
        private const int ClientId = 1, OtherClientId = 2, VetId = 3, PetId = 10, OtherPetId = 11;

        private readonly FakeConsultationRepository _consultationRepository = new();
        private readonly FakePetRepository _petRepository = new();
        private readonly FakeUserRepository _userRepository = new();
        private readonly ConsultationServices _sut;

        public ConsultationServicesTests()
        {
            _userRepository.Items.AddRange(new User[]
            {
                new Client { Id = ClientId, FullName = "Client", Email = "c1@test.com", Phone = "1", Password = "x", Dni = "1" },
                new Client { Id = OtherClientId, FullName = "Client 2", Email = "c2@test.com", Phone = "1", Password = "x", Dni = "2" },
                new Veterinarian { Id = VetId, FullName = "Vet", Email = "vet@test.com", Phone = "1", Password = "x" }
            });
            _petRepository.Items.AddRange(new[]
            {
                new Pet { Id = PetId, Name = "Mine", Specie = "Dog", Breed = "Mixed", BirthDate = new DateOnly(2020, 1, 1), ClientId = ClientId },
                new Pet { Id = OtherPetId, Name = "Theirs", Specie = "Cat", Breed = "Mixed", BirthDate = new DateOnly(2020, 1, 1), ClientId = OtherClientId }
            });
            _sut = new ConsultationServices(_consultationRepository, _petRepository, _userRepository);
        }

        private static Consultation NewConsultation(int id, int petId) => new()
        {
            Id = id, Description = "Checkup", Date = new DateTime(2026, 10, 1, 10, 0, 0), PetId = petId, VeterinarianId = VetId
        };

        private static CreateConsultationDTO NewDto(int petId, int veterinarianId) => new()
        {
            Description = "Vaccine", Date = new DateTime(2026, 11, 1, 9, 30, 0), PetId = petId, VeterinarianId = veterinarianId
        };

        [Fact]
        public async Task GetAllConsultation_WhenNone_ReturnsEmptyList()
        {
            // Act
            var consultations = await _sut.GetAllConsultationAsync();

            // Assert
            Assert.Empty(consultations);
        }

        [Fact]
        public async Task GetConsultationById_WhenMissing_ThrowsNotFound()
        {
            // Act
            var act = () => _sut.GetConsultationByIdAsync(99);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task AddConsultation_WithValidPetAndVet_CreatesConsultationKeepingTime()
        {
            // Act
            var created = await _sut.AddConsultationAsync(NewDto(PetId, VetId));

            // Assert
            var stored = Assert.Single(_consultationRepository.Items);
            Assert.Equal(stored.Id, created.Id);
            Assert.Equal(new DateTime(2026, 11, 1, 9, 30, 0), stored.Date);
        }

        [Fact]
        public async Task AddConsultation_WhenPetMissing_ThrowsBadRequest()
        {
            // Act
            var act = () => _sut.AddConsultationAsync(NewDto(99, VetId));

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task AddConsultation_WhenVeterinarianIdIsNotAVet_ThrowsBadRequest()
        {
            // Act
            var act = () => _sut.AddConsultationAsync(NewDto(PetId, ClientId));

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task UpdateConsultation_WhenMissing_ThrowsNotFound()
        {
            // Arrange
            var dto = new ConsultationDTO { Description = "X", Date = DateTime.Now, PetId = PetId, VeterinarianId = VetId };

            // Act
            var act = () => _sut.UpdateConsultationAsync(99, dto);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task UpdateConsultation_WithValidData_ChangesStoredConsultation()
        {
            // Arrange
            _consultationRepository.Items.Add(NewConsultation(1, PetId));
            var dto = new ConsultationDTO { Description = "Surgery follow-up", Date = new DateTime(2026, 12, 1, 15, 0, 0), PetId = OtherPetId, VeterinarianId = VetId };

            // Act
            await _sut.UpdateConsultationAsync(1, dto);

            // Assert
            var stored = _consultationRepository.Items.Single();
            Assert.Equal("Surgery follow-up", stored.Description);
            Assert.Equal(OtherPetId, stored.PetId);
        }

        [Fact]
        public async Task UpdateConsultation_WhenVeterinarianIdIsNotAVet_ThrowsBadRequest()
        {
            // Arrange
            _consultationRepository.Items.Add(NewConsultation(1, PetId));
            var dto = new ConsultationDTO { Description = "X", Date = DateTime.Now, PetId = PetId, VeterinarianId = ClientId };

            // Act
            var act = () => _sut.UpdateConsultationAsync(1, dto);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task DeleteConsultation_WhenExists_RemovesConsultation()
        {
            // Arrange
            _consultationRepository.Items.Add(NewConsultation(1, PetId));

            // Act
            await _sut.DeleteConsultationAsync(1);

            // Assert
            Assert.Empty(_consultationRepository.Items);
        }

        [Fact]
        public async Task DeleteConsultation_WhenMissing_ThrowsNotFound()
        {
            // Act
            var act = () => _sut.DeleteConsultationAsync(99);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task GetConsultationsByPetId_WhenClientOwnsPet_ReturnsConsultations()
        {
            // Arrange
            _consultationRepository.Items.AddRange(new[] { NewConsultation(1, PetId), NewConsultation(2, OtherPetId) });

            // Act
            var consultations = await _sut.GetConsultationsByPetIdAsync(PetId, ClientId);

            // Assert
            Assert.Equal(1, Assert.Single(consultations).Id);
        }

        [Fact]
        public async Task GetConsultationsByPetId_WhenClientDoesNotOwnPet_ThrowsNotFound()
        {
            // Arrange
            _consultationRepository.Items.Add(NewConsultation(1, OtherPetId));

            // Act
            var act = () => _sut.GetConsultationsByPetIdAsync(OtherPetId, ClientId);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task GetConsultationsByPetId_WhenStaff_ReturnsAnyPetsConsultations()
        {
            // Arrange
            _consultationRepository.Items.Add(NewConsultation(1, OtherPetId));

            // Act
            var consultations = await _sut.GetConsultationsByPetIdAsync(OtherPetId);

            // Assert
            Assert.Single(consultations);
        }

        [Fact]
        public async Task GetConsultationsByVeterinarianId_ReturnsOnlyThatVetsConsultations()
        {
            // Arrange
            var other = NewConsultation(2, PetId);
            other.VeterinarianId = 99;
            _consultationRepository.Items.AddRange(new[] { NewConsultation(1, PetId), other });

            // Act
            var consultations = await _sut.GetConsultationsByVeterinarianIdAsync(VetId);

            // Assert
            Assert.Equal(1, Assert.Single(consultations).Id);
        }
    }
}
