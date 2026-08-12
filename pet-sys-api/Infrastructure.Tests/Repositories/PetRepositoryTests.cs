using Infrastructure.Repositories;
using Infrastructure.Tests.TestHelpers;

namespace Infrastructure.Tests.Repositories
{
    public class PetRepositoryTests
    {
        [Fact]
        public async Task GetByClientIdAsync_ReturnsOnlyPetsForThatClient()
        {
            using var context = InMemoryDbContextFactory.Create();
            var owner = EntityFactory.CreateClient(email: "owner@test.com", dni: "11111111");
            var otherOwner = EntityFactory.CreateClient(email: "other@test.com", dni: "22222222");
            context.Clients.AddRange(owner, otherOwner);
            await context.SaveChangesAsync();
            var repository = new PetRepository(context);
            var ownerPet = await repository.AddAsync(EntityFactory.CreatePet(owner.Id, "Firulais"));
            await repository.AddAsync(EntityFactory.CreatePet(otherOwner.Id, "Michi"));

            var found = await repository.GetByClientIdAsync(owner.Id);

            var match = Assert.Single(found);
            Assert.Equal(ownerPet.Id, match.Id);
        }

        [Fact]
        public async Task GetByClientIdAsync_NoPetsForClient_ReturnsEmpty()
        {
            using var context = InMemoryDbContextFactory.Create();
            var owner = EntityFactory.CreateClient();
            context.Clients.Add(owner);
            await context.SaveChangesAsync();
            var repository = new PetRepository(context);

            var found = await repository.GetByClientIdAsync(owner.Id);

            Assert.Empty(found);
        }
    }
}
