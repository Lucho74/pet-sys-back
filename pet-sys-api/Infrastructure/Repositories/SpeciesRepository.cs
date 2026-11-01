using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
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
            return await _context.Set<Pet>().AnyAsync(p => p.SpeciesId == id);
        }

        public async Task<bool> HasBreedsAsync(int id)
        {
            return await _context.Set<Breed>().AnyAsync(b => b.SpeciesId == id);
        }

        public async Task DeleteAsync(Species species)
        {
            _context.Set<Species>().Remove(species);
            await _context.SaveChangesAsync();
        }
    }
}
