using Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISpeciesServices
    {
        Task<IEnumerable<SpeciesDTO>> GetAllSpeciesAsync();
        Task<SpeciesDTO> GetSpeciesByIdAsync(int id);
        Task<SpeciesDTO> AddSpeciesAsync(CreateEditSpeciesDTO dto);
        Task<SpeciesDTO> UpdateSpeciesAsync(int id, CreateEditSpeciesDTO dto);
        Task DeleteSpeciesAsync(int id);
    }
}
