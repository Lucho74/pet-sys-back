using Domain.Common;
using Domain.Entities;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ISpeciesRepository : IBaseRepository<Species>
    {
        Task<Species?> GetByNameAsync(string name);
        Task<bool> HasPetsAsync(int id);
        Task DeleteAsync(Species species);
        Task<PaginatedList<Species>> GetPagedAsync(PaginationQuery q);
    }
}
