using Application.Models;

namespace Application.Interfaces
{
    public interface IPetServices
    {
        Task<IEnumerable<PetDTO>> GetAllPetAsync();
        Task<IEnumerable<PetDTO>> GetPetsByClientAsync(int clientId);
        Task<PetDTO> GetPetByIdAsync(int id);
        Task<PetDTO> AddPetAsync(CreateEditPetDTO dto);
        Task<PetDTO> UpdatePetAsync(int id, CreateEditPetDTO dto);
    }
}
