using Application.Models;
using Domain.Common;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IConsultationServices
    {
        Task<IEnumerable<ConsultationDTO>> GetAllConsultationAsync();
        Task<ConsultationDTO> GetConsultationByIdAsync(int id);
        Task<ConsultationDTO> AddConsultationAsync(CreateConsultationDTO dto, int veterinarianId);
        Task<ConsultationDTO> UpdateConsultationAsync(int id, UpdateConsultationDTO dto);
        Task<IEnumerable<ConsultationDTO>> GetConsultationsByPetIdAsync(int petId);
        Task<IEnumerable<ConsultationDTO>> GetConsultationsByVeterinarianIdAsync(int veterinarianId);
        Task SuspendConsultationAsync(int id);
        Task ReactivateConsultationAsync(int id);
        Task<PaginatedList<ConsultationDTO>> GetConsultationsPagedAsync(PaginationQuery q);
        Task UploadAttachmentAsync(int id, Stream fileStream, string contentType, CancellationToken cancellationToken = default);
        Task DeleteAttachmentAsync(int id, string blobName, CancellationToken cancellationToken = default);
    }
}
