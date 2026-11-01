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

        public async Task DeleteAsync(Breed breed)
        {
            _context.Set<Breed>().Remove(breed);
            await _context.SaveChangesAsync();
        }
    }
}
