using Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPetServices
    {
        Task<IEnumerable<PetDTO>> GetAllPetAsync();
        Task<PetDTO> GetPetByIdAsync(int id);
        Task<PetDTO> AddPetAsync(CreatePetDTO dto);
        Task<PetDTO> UpdatePetAsync(int id, PetDTO dto);
        Task DeletePetAsync(int id);
    }
}
