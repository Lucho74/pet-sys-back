using Application.Models;
using Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBreedServices
    {
        Task<PaginatedList<BreedDTO>> GetBreedsPagedAsync(BreedQuery q);
        Task<IEnumerable<BreedDTO>> GetBreedsBySpeciesIdAsync(int speciesId);
        Task<BreedDTO> GetBreedByIdAsync(int id);
        Task<BreedDTO> AddBreedAsync(CreateBreedDTO dto);
        Task<BreedDTO> UpdateBreedAsync(int id, UpdateBreedDTO dto);
        Task DeleteBreedAsync(int id);
    }
}
