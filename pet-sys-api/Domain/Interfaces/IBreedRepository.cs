using Domain.Common;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IBreedRepository : IBaseRepository<Breed>
    {
        Task<IEnumerable<Breed>> GetBySpeciesIdAsync(int speciesId);
        Task<Breed?> GetByNameAsync(int speciesId, string name);
        Task<bool> HasPetsAsync(int id);
        Task<int> CountBySpeciesIdAsync(int speciesId);
        Task DeleteAsync(Breed breed);
        Task<PaginatedList<Breed>> GetPagedAsync(BreedQuery q);
    }
}
