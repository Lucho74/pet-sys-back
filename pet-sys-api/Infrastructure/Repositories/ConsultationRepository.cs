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

        public async Task<IEnumerable<Consultation>> GetByPetIdAsync(int petId)
        {
            return await _context.Set<Consultation>()
                .Where(c => c.PetId == petId)
                .ToListAsync();
        }
    }
}
