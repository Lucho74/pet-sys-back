using Application.Models;
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
        Task DeletePetAsync(int id);


    }
}
