using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IConsultationRepository : IBaseRepository<Consultation>
    {
        Task<IEnumerable<Consultation>> GetByPetIdAsync(int petId);
        Task<IEnumerable<Consultation>> GetByVeterinarianIdAsync(int veterinarianId);
    }
}
