using Domain.Entities;

namespace Infrastructure.Tests.TestHelpers
{
    internal static class EntityFactory
    {
        public static Client CreateClient(string email = "client@test.com", string dni = "12345678") => new()
        {
            FullName = "Client Test",
            Password = "Password1",
            Email = email,
            Phone = "1234567890",
            Dni = dni,
        };

        public static Veterinarian CreateVeterinarian(string email = "vet@test.com") => new()
        {
            FullName = "Vet Test",
            Password = "Password1",
            Email = email,
            Phone = "1234567890",
        };

        public static Admin CreateAdmin(string email = "admin@test.com") => new()
        {
            FullName = "Admin Test",
            Password = "Password1",
            Email = email,
            Phone = "1234567890",
        };

        public static Pet CreatePet(int clientId, string name = "Firulais") => new()
        {
            Name = name,
            Specie = "Dog",
            Breed = "Labrador",
            BirthDate = new DateOnly(2020, 1, 1),
            ClientId = clientId,
        };

        public static Consultation CreateConsultation(int petId, int veterinarianId, StatusConsultation status = StatusConsultation.Pending) => new()
        {
            Description = "Chequeo general",
            Date = new DateTime(2026, 1, 1),
            Status = status,
            PetId = petId,
            VeterinarianId = veterinarianId,
        };
    }
}
