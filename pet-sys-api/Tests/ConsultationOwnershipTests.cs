using System.Security.Claims;
using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;

namespace Tests
{
    [Trait("Category", "Unit")]
    public class ConsultationOwnershipTests
    {
        // Hand-written mock: the test asserts the interaction (which clientId reached the service), not state.
        private class RecordingConsultationServices : IConsultationServices
        {
            public int? ReceivedClientId { get; private set; }

            public Task<IEnumerable<ConsultationDTO>> GetConsultationsByPetIdAsync(int petId, int? clientId = null)
            {
                ReceivedClientId = clientId;
                return Task.FromResult(Enumerable.Empty<ConsultationDTO>());
            }

            public Task<IEnumerable<ConsultationDTO>> GetAllConsultationAsync() => throw new NotImplementedException();
            public Task<ConsultationDTO> GetConsultationByIdAsync(int id) => throw new NotImplementedException();
            public Task<ConsultationDTO> AddConsultationAsync(CreateConsultationDTO dto) => throw new NotImplementedException();
            public Task<ConsultationDTO> UpdateConsultationAsync(int id, ConsultationDTO dto) => throw new NotImplementedException();
            public Task DeleteConsultationAsync(int id) => throw new NotImplementedException();
            public Task<IEnumerable<ConsultationDTO>> GetConsultationsByVeterinarianIdAsync(int veterinarianId) => throw new NotImplementedException();
        }

        private static ConsultationController ControllerFor(IConsultationServices services, string role, int userId)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            }, "Test");

            return new ConsultationController(services)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } }
            };
        }

        [Fact]
        public async Task GetByPetId_WhenClient_PassesOwnIdToService()
        {
            // Arrange
            var services = new RecordingConsultationServices();
            var sut = ControllerFor(services, "Client", 7);

            // Act
            await sut.GetByPetId(1);

            // Assert
            Assert.Equal(7, services.ReceivedClientId);
        }

        [Fact]
        public async Task GetByPetId_WhenStaff_PassesNoClientId()
        {
            // Arrange
            var services = new RecordingConsultationServices();
            var sut = ControllerFor(services, "Veterinarian", 3);

            // Act
            await sut.GetByPetId(1);

            // Assert
            Assert.Null(services.ReceivedClientId);
        }
    }
}
