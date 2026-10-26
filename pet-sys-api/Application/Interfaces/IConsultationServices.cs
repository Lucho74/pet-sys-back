using Application.Models;

namespace Application.Interfaces
{
    public interface IConsultationServices
    {
        Task<IEnumerable<ConsultationDTO>> GetAllConsultationAsync();
        Task<ConsultationDTO> GetConsultationByIdAsync(int id);
        Task<ConsultationDTO> AddConsultationAsync(CreateEditConsultationDTO dto);
        Task<ConsultationDTO> UpdateConsultationAsync(int id, CreateEditConsultationDTO dto);
        Task<IEnumerable<ConsultationDTO>> GetConsultationsByPetIdAsync(int petId, int? clientId = null);
        Task<IEnumerable<ConsultationDTO>> GetConsultationsByVeterinarianIdAsync(int veterinarianId);
    }
}
