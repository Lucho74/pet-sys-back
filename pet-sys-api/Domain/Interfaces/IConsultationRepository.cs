using Domain.Common;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IConsultationRepository : IBaseRepository<Consultation>
    {
        Task<Consultation?> GetByIdWithParticipantsAsync(int id);
        Task<IEnumerable<Consultation>> GetByPetIdAsync(int petId);
        Task<IEnumerable<Consultation>> GetByVeterinarianIdAsync(int veterinarianId);
        Task<PaginatedList<Consultation>> GetPagedAsync(PaginationQuery q);
        Task<IEnumerable<Consultation>> GetRecentAsync(int take);
    }
}
