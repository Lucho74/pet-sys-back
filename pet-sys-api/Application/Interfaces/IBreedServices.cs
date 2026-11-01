using Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBreedServices
    {
        Task<IEnumerable<BreedDTO>> GetAllBreedAsync();
        Task<IEnumerable<BreedDTO>> GetBreedsBySpeciesIdAsync(int speciesId);
        Task<BreedDTO> GetBreedByIdAsync(int id);
        Task<BreedDTO> AddBreedAsync(CreateEditBreedDTO dto);
        Task<BreedDTO> UpdateBreedAsync(int id, CreateEditBreedDTO dto);
        Task DeleteBreedAsync(int id);
    }
}
