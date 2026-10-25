using Domain.Entities;
using Domain.Interfaces;

namespace Tests
{
    public class FakeUserRepository : FakeRepository<User>, IUserRepository
    {
        public Task<User?> GetByEmailAsync(string email) => Task.FromResult(Items.FirstOrDefault(u => u.Email == email));
        public Task<IEnumerable<T>> GetByRoleAsync<T>() where T : User => Task.FromResult<IEnumerable<T>>(Items.OfType<T>().ToList());
    }

    public class FakePetRepository : FakeRepository<Pet>, IPetRepository
    {
        public Task<IEnumerable<Pet>> GetByClientIdAsync(int clientId) => Task.FromResult<IEnumerable<Pet>>(Items.Where(p => p.ClientId == clientId).ToList());
    }

    public class FakeConsultationRepository : FakeRepository<Consultation>, IConsultationRepository
    {
        public Task<IEnumerable<Consultation>> GetByPetIdAsync(int petId) => Task.FromResult<IEnumerable<Consultation>>(Items.Where(c => c.PetId == petId).ToList());
        public Task<IEnumerable<Consultation>> GetByVeterinarianIdAsync(int veterinarianId) => Task.FromResult<IEnumerable<Consultation>>(Items.Where(c => c.VeterinarianId == veterinarianId).ToList());
    }
}
