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

        protected override IQueryable<Pet> Query => _context.Set<Pet>()
            .Include(p => p.Breed)
                .ThenInclude(b => b.Species);

        public async Task<IEnumerable<Pet>> GetRecentAsync(int take)
        {
            return await Query
                .Include(p => p.Client)
                    .ThenInclude(c => c.User)
                .OrderByDescending(p => p.Id)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pet>> GetByClientIdAsync(int clientId)
        {
            return await Query
            .Where(p => p.ClientId == clientId)
            .ToListAsync();
        }

        public async Task<PaginatedList<Pet>> GetPagedAsync(PetQuery q)
        {
            var query = Query;

            if (!q.IncludeSuspended)
                query = query.Where(p => !p.IsSuspended);

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var pattern = $"%{q.Search.Trim()}%";
                query = query.Where(p => EF.Functions.Like(p.Name, pattern));
            }

            if (q.SpeciesId.HasValue)
                query = query.Where(p => p.Breed.SpeciesId == q.SpeciesId.Value);

            if (q.BreedId.HasValue)
                query = query.Where(p => p.BreedId == q.BreedId.Value);

            query = q.SortBy?.ToLowerInvariant() switch
            {
                "name" => ApplyOrder(query, p => p.Name, q.SortDesc),
                "species" => ApplyOrder(query, p => p.Breed.Species.Name, q.SortDesc),
                "breed" => ApplyOrder(query, p => p.Breed.Name, q.SortDesc),
                _ => ApplyOrder(query, p => p.Id, q.SortDesc)
            };

            return await Paginate(query, q.PageIndex, q.PageSize);
        }
    }
}
