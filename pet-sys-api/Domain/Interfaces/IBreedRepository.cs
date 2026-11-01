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
        Task DeleteAsync(Breed breed);
    }
}
