using Application.Models;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IConsultationServices
    {
        Task<IEnumerable<ConsultationDTO>> GetAllConsultationAsync();
        Task<ConsultationDTO> GetConsultationByIdAsync(int id);
        Task<ConsultationDTO> AddConsultationAsync(CreateConsultationDTO dto);
        Task<ConsultationDTO> UpdateConsultationAsync(int id, ConsultationDTO dto);
        Task DeleteConsultationAsync(int id);
        Task<IEnumerable<ConsultationDTO>> GetConsultationsByPetIdAsync(int petId);
        Task<IEnumerable<ConsultationDTO>> GetConsultationsByVeterinarianIdAsync(int veterinarianId);
        Task<IEnumerable<ConsultationDTO>> GetConsultationsByStatusAsync(StatusConsultation status);
    }
}
