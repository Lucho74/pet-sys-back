using Application.Models;
using Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPetServices
    {
        Task<IEnumerable<PetDTO>> GetPetsByClientAsync(int clientId);
        Task<PetDTO> GetPetByIdAsync(int id);
        Task<PetDTO> AddPetAsync(CreateEditPetDTO dto);
        Task<PetDTO> UpdatePetAsync(int id, CreateEditPetDTO dto);
        Task SuspendPetAsync(int id);
        Task ReactivatePetAsync(int id);
        Task<PaginatedList<PetDTO>> GetPetsPagedAsync(PetQuery q);
        Task UploadImageAsync(int petId, Stream imageStream, string contentType, CancellationToken cancellationToken = default);
        Task DeleteImageAsync(int petId, CancellationToken cancellationToken = default);

    }
}
