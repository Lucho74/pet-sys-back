using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SpeciesRepository : BaseRepository<Species>, ISpeciesRepository
    {
        public SpeciesRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Species?> GetByNameAsync(string name)
        {
            return await _context.Set<Species>().FirstOrDefaultAsync(s => s.Name == name);
        }

        public async Task<bool> HasPetsAsync(int id)
        {
            return await _context.Set<Pet>().AnyAsync(p => p.Breed.SpeciesId == id);
        }

        public async Task DeleteAsync(Species species)
        {
            _context.Set<Species>().Remove(species);
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedList<Species>> GetPagedAsync(PaginationQuery q)
        {
            var query = _context.Set<Species>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var pattern = $"%{q.Search.Trim()}%";
                query = query.Where(s => EF.Functions.Like(s.Name, pattern));
            }

            query = q.SortBy?.ToLowerInvariant() switch
            {
                "name" => ApplyOrder(query, s => s.Name, q.SortDesc),
                _ => ApplyOrder(query, s => s.Id, q.SortDesc)
            };

            return await Paginate(query, q.PageIndex, q.PageSize);
        }
    }
}
