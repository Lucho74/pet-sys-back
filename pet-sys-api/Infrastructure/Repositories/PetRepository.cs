using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PetRepository : BaseRepository<Pet>, IPetRepository
    {
        public PetRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Pet>> GetByClientIdAsync(int clientId)
        {
            return await _context.Set<Pet>()
            .Where(p => p.ClientId == clientId)
            .ToListAsync();
        }
    }
}
