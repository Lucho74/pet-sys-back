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
    public class ConsultationRepository : BaseRepository<Consultation>, IConsultationRepository
    {
        public ConsultationRepository(ApplicationDbContext context) : base(context)
        {
        }

        private IQueryable<Consultation> QueryWithParticipants => Query
            .Include(c => c.Pet)
            .Include(c => c.Veterinarian)
                .ThenInclude(v => v.User);

        public async Task<Consultation?> GetByIdWithParticipantsAsync(int id)
        {
            return await QueryWithParticipants.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Consultation>> GetRecentAsync(int take)
        {
            return await _context.Set<Consultation>()
                .Include(c => c.Pet)
                .Include(c => c.Veterinarian)
                    .ThenInclude(v => v.User)
                .OrderByDescending(c => c.Date)
                .ThenByDescending(c => c.Id)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Consultation>> GetByVeterinarianIdAsync(int veterinarianId)
        {
            return await _context.Set<Consultation>()
                .Where(c => c.VeterinarianId == veterinarianId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Consultation>> GetByPetIdAsync(int petId)
        {
            return await _context.Set<Consultation>()
                .Where(c => c.PetId == petId)
                .ToListAsync();
        }

        public async Task<PaginatedList<Consultation>> GetPagedAsync(PaginationQuery q, int? veterinarianId = null)
        {
            var query = QueryWithParticipants;

            if (veterinarianId is int responsibleId)
                query = query.Where(c => c.VeterinarianId == responsibleId);

            if (!q.IncludeSuspended)
                query = query.Where(c => !c.IsSuspended);

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var pattern = $"%{q.Search.Trim()}%";
                query = query.Where(c => EF.Functions.Like(c.Description, pattern));
            }

            query = q.SortBy?.ToLowerInvariant() switch
            {
                "date" => ApplyOrder(query, c => c.Date, q.SortDesc),
                _ => ApplyOrder(query, c => c.Id, q.SortDesc)
            };

            return await Paginate(query, q.PageIndex, q.PageSize);
        }
    }
}
