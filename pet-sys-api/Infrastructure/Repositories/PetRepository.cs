using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PetRepository : BaseRepository<Pet>, IPetRepository
    {
        public PetRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Pet?> GetByIdAsync(int id)
        {
            return await _context.Set<Pet>()
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public override async Task<IEnumerable<Pet>> GetAllAsync()
        {
            return await _context.Set<Pet>()
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pet>> GetByClientIdAsync(int clientId)
        {
            return await _context.Set<Pet>()
            .Include(p => p.Species)
            .Include(p => p.Breed)
            .Where(p => p.ClientId == clientId)
            .ToListAsync();
        }
    }
}
