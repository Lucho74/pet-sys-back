using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Tests.TestHelpers;

namespace Infrastructure.Tests.Repositories
{
    public class ConsultationRepositoryTests
    {
        private static async Task<(Pet pet, Veterinarian vet)> SeedPetAndVeterinarianAsync(
            Infrastructure.Context.ApplicationDbContext context)
        {
            var client = EntityFactory.CreateClient();
            var vet = EntityFactory.CreateVeterinarian();
            context.Clients.Add(client);
            context.Veterinarians.Add(vet);
            await context.SaveChangesAsync();

            var pet = EntityFactory.CreatePet(client.Id);
            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            return (pet, vet);
        }

        [Fact]
        public async Task GetByStatusAsync_ReturnsOnlyMatchingStatus()
        {
            using var context = InMemoryDbContextFactory.Create();
            var (pet, vet) = await SeedPetAndVeterinarianAsync(context);
            var repository = new ConsultationRepository(context);
            var pending = await repository.AddAsync(
                EntityFactory.CreateConsultation(pet.Id, vet.Id, StatusConsultation.Pending));
            await repository.AddAsync(
                EntityFactory.CreateConsultation(pet.Id, vet.Id, StatusConsultation.Completed));

            var found = await repository.GetByStatusAsync(StatusConsultation.Pending);

            var match = Assert.Single(found);
            Assert.Equal(pending.Id, match.Id);
        }

        [Fact]
        public async Task GetByVeterinarianIdAsync_ReturnsOnlyConsultationsForThatVeterinarian()
        {
            using var context = InMemoryDbContextFactory.Create();
            var (pet, vet) = await SeedPetAndVeterinarianAsync(context);
            var otherVet = EntityFactory.CreateVeterinarian(email: "other-vet@test.com");
            context.Veterinarians.Add(otherVet);
            await context.SaveChangesAsync();
            var repository = new ConsultationRepository(context);
            var vetConsultation = await repository.AddAsync(EntityFactory.CreateConsultation(pet.Id, vet.Id));
            await repository.AddAsync(EntityFactory.CreateConsultation(pet.Id, otherVet.Id));

            var found = await repository.GetByVeterinarianIdAsync(vet.Id);

            var match = Assert.Single(found);
            Assert.Equal(vetConsultation.Id, match.Id);
        }
    }
}
