using Application.Models;
using Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPetServices
    {
        Task<IEnumerable<PetDTO>> GetAllPetAsync();
        Task<IEnumerable<PetDTO>> GetPetsByClientAsync(int clientId);
        Task<PetDTO> GetPetByIdAsync(int id);
        Task<PetDTO> AddPetAsync(CreateEditPetDTO dto);
        Task<PetDTO> UpdatePetAsync(int id, CreateEditPetDTO dto);
        Task<PaginatedList<PetDTO>> ListAllByPaginationAsync(int pageIndex, int pageSize);


    }
}
