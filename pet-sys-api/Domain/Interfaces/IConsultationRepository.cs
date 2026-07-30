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
        Task<IEnumerable<Consultation>> GetByVeterinarianIdAsync(string veterinarianId);
        Task<IEnumerable<Consultation>> GetByStatusAsync(StatusConsultation status);
    }
}
