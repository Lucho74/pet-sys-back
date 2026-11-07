using Domain.Common;
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

        public async Task<IEnumerable<Pet>> GetByClientIdAsync(int clientId)
        {
            return await _context.Set<Pet>()
            .Where(p => p.ClientId == clientId)
            .ToListAsync();
        }

        public async Task<PaginatedList<Pet>> GetPagedAsync(PetQuery q)
        {
            var query = _context.Set<Pet>().AsQueryable();

            if (!q.IncludeSuspended)
                query = query.Where(p => !p.IsSuspended);

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var pattern = $"%{q.Search.Trim()}%";
                query = query.Where(p => EF.Functions.Like(p.Name, pattern));
            }

            if (!string.IsNullOrWhiteSpace(q.Specie))
            {
                var specie = q.Specie.Trim();
                query = query.Where(p => p.Specie == specie);
            }

            if (!string.IsNullOrWhiteSpace(q.Breed))
            {
                var breed = q.Breed.Trim();
                query = query.Where(p => p.Breed == breed);
            }

            query = q.SortBy?.ToLowerInvariant() switch
            {
                "name" => ApplyOrder(query, p => p.Name, q.SortDesc),
                "specie" => ApplyOrder(query, p => p.Specie, q.SortDesc),
                "breed" => ApplyOrder(query, p => p.Breed, q.SortDesc),
                _ => ApplyOrder(query, p => p.Id, q.SortDesc)
            };

            return await Paginate(query, q.PageIndex, q.PageSize);
        }
    }
}
