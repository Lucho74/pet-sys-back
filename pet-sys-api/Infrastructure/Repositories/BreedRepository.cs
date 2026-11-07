using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class BreedRepository : BaseRepository<Breed>, IBreedRepository
    {
        public BreedRepository(ApplicationDbContext context) : base(context)
        {
        }

        protected override IQueryable<Breed> Query => _context.Set<Breed>()
            .Include(b => b.Species);

        public async Task<IEnumerable<Breed>> GetBySpeciesIdAsync(int speciesId)
        {
            return await _context.Set<Breed>()
                .Where(b => b.SpeciesId == speciesId)
                .ToListAsync();
        }

        public async Task<Breed?> GetByNameAsync(int speciesId, string name)
        {
            return await _context.Set<Breed>().FirstOrDefaultAsync(b => b.SpeciesId == speciesId && b.Name == name);
        }

        public async Task<bool> HasPetsAsync(int id)
        {
            return await _context.Set<Pet>().AnyAsync(p => p.BreedId == id);
        }

        public async Task<int> CountBySpeciesIdAsync(int speciesId)
        {
            return await _context.Set<Breed>().CountAsync(b => b.SpeciesId == speciesId);
        }

        public async Task DeleteAsync(Breed breed)
        {
            _context.Set<Breed>().Remove(breed);
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedList<Breed>> GetPagedAsync(BreedQuery q)
        {
            var query = _context.Set<Breed>().AsQueryable();

            if (q.SpeciesId.HasValue)
                query = query.Where(b => b.SpeciesId == q.SpeciesId.Value);

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var pattern = $"%{q.Search.Trim()}%";
                query = query.Where(b => EF.Functions.Like(b.Name, pattern));
            }

            query = q.SortBy?.ToLowerInvariant() switch
            {
                "name" => ApplyOrder(query, b => b.Name, q.SortDesc),
                "species" => ApplyOrder(query, b => b.Species.Name, q.SortDesc),
                _ => ApplyOrder(query, b => b.Id, q.SortDesc)
            };

            return await Paginate(query, q.PageIndex, q.PageSize);
        }
    }
}
