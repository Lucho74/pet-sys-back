using Application.Exceptions;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class VeterinarianServices : IVeterinarianServices
    {
        public readonly IUserRepository _userRepository;

        public VeterinarianServices(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<WorkShiftDTO>> GetWorkShiftsAsync(int veterinarianId)
        {
            if (await _userRepository.GetByIdAsync(veterinarianId) is not Veterinarian veterinarian)
            {
                throw new NotFoundException($"Veterinarian with id {veterinarianId} was not found.");
            }

            if (veterinarian.WorkShifts == null)
            {
                return new List<WorkShiftDTO>();
            }

            return veterinarian.WorkShifts.Select(shift => new WorkShiftDTO
            {
                Day = shift.Day,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime
            });
        }

        public async Task<IEnumerable<WorkShiftDTO>> UpdateWorkShiftsAsync(int veterinarianId, IEnumerable<WorkShiftDTO> shifts)
        {
            if (await _userRepository.GetByIdAsync(veterinarianId) is not Veterinarian veterinarian)
            {
                throw new NotFoundException($"Veterinarian with id {veterinarianId} was not found.");
            }

            var shiftList = shifts.ToList();
            foreach (var shift in shiftList)
            {
                if (shift.EndTime <= shift.StartTime)
                {
                    throw new BadRequestException($"The {shift.Day} work shift must end after it starts. Split an overnight shift into two shifts, one per day.");
                }
            }

            veterinarian.WorkShifts = shiftList.Select(shift => new WorkShift
            {
                Day = shift.Day,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime
            }).ToList();

            var updated = await _userRepository.UpdateAsync(veterinarianId, veterinarian);
            if (updated == null)
            {
                throw new NotFoundException($"Veterinarian with id {veterinarianId} was not found.");
            }

            return shiftList;
        }
    }
}
