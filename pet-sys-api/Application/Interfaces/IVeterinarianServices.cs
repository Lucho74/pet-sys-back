using Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IVeterinarianServices
    {
        Task<IEnumerable<WorkShiftDTO>> GetWorkShiftsAsync(int veterinarianId);
        Task<IEnumerable<WorkShiftDTO>> UpdateWorkShiftsAsync(int veterinarianId, IEnumerable<WorkShiftDTO> shifts);
    }
}
