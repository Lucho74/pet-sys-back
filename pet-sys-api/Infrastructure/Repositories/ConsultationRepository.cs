using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    internal class ConsultationRepository : BaseRepository<Consultation>, IConsultationRepository
    {
        public ConsultationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Consultation>> GetByStatusAsync(StatusConsultation status)
        {
            return await _context.Set<Consultation>()
                .Where(c => c.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Consultation>> GetByVeterinarianIdAsync(int veterinarianId)
        {
            return await _context.Set<Consultation>()
                .Where(c => c.VeterinarianId == veterinarianId)
                .ToListAsync();
        }
    }
}
