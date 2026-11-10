using Application.Models;
using Domain.Common;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IConsultationServices
    {
        Task<ConsultationWithParticipantsDTO> GetConsultationByIdAsync(int id);
        Task<ConsultationDTO> AddConsultationAsync(CreateConsultationDTO dto, int veterinarianId, bool veterinarianOnly = false);
        Task<ConsultationDTO> UpdateConsultationAsync(int id, UpdateConsultationDTO dto, bool veterinarianOnly = false);
        Task<IEnumerable<ConsultationDTO>> GetConsultationsByPetIdAsync(int petId);
        Task<IEnumerable<ConsultationDTO>> GetConsultationsByVeterinarianIdAsync(int veterinarianId);
        Task SuspendConsultationAsync(int id);
        Task ReactivateConsultationAsync(int id);
        Task<PaginatedList<ConsultationWithParticipantsDTO>> GetConsultationsPagedAsync(PaginationQuery q, int? responsibleUserId = null);
        Task UploadAttachmentAsync(int id, Stream fileStream, string contentType, string fileName, CancellationToken cancellationToken = default);
        Task DeleteAttachmentAsync(int id, string blobName, CancellationToken cancellationToken = default);
    }
}
