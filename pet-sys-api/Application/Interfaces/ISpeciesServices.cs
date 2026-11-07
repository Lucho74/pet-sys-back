using Application.Models;
using Domain.Common;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISpeciesServices
    {
        Task<PaginatedList<SpeciesDTO>> GetSpeciesPagedAsync(PaginationQuery q);
        Task<SpeciesDTO> GetSpeciesByIdAsync(int id);
        Task<SpeciesDTO> AddSpeciesAsync(CreateEditSpeciesDTO dto);
        Task<SpeciesDTO> UpdateSpeciesAsync(int id, CreateEditSpeciesDTO dto);
        Task DeleteSpeciesAsync(int id);
    }
}
