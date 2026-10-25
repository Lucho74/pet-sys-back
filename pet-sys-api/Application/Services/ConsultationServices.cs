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
    public class ConsultationServices : IConsultationServices
    {
        public readonly IConsultationRepository _consultationRepository;
        public readonly IPetRepository _petRepository;
        public readonly IUserRepository _userRepository;

        public ConsultationServices(IConsultationRepository consultationRepository, IPetRepository petRepository, IUserRepository userRepository)
        {
            _consultationRepository = consultationRepository;
            _petRepository = petRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<ConsultationDTO>> GetAllConsultationAsync()
        {
            var consultations = await _consultationRepository.GetAllAsync();
            var consultationList = consultations.ToList();
            if (consultationList.Count == 0)
            {
                throw new NotFoundException("No consultations found.");
            }

            return consultationList.Select(consultation => new ConsultationDTO
            {
                Id = consultation.Id,
                Description = consultation.Description,
                Date = consultation.Date,
                PetId = consultation.PetId,
                VeterinarianId = consultation.VeterinarianId
            });
        }

        public async Task<ConsultationDTO> GetConsultationByIdAsync(int id)
        {
            var consultation = await _consultationRepository.GetByIdAsync(id);
            if (consultation == null)
            {
                throw new NotFoundException($"Consultation with id {id} was not found.");
            }

            return new ConsultationDTO
            {
                Id = consultation.Id,
                Description = consultation.Description,
                Date = consultation.Date,
                PetId = consultation.PetId,
                VeterinarianId = consultation.VeterinarianId
            };
        }

        public async Task<ConsultationDTO> AddConsultationAsync(CreateConsultationDTO dto)
        {
            if (await _petRepository.GetByIdAsync(dto.PetId) == null)
            {
                throw new BadRequestException("PetId does not reference an existing pet.");
            }

            if (await _userRepository.GetByIdAsync(dto.VeterinarianId) is not Veterinarian)
            {
                throw new BadRequestException("VeterinarianId does not reference an existing veterinarian.");
            }

            var consultation = new Consultation
            {
                Description = dto.Description,
                Date = dto.Date,
                PetId = dto.PetId,
                VeterinarianId = dto.VeterinarianId
            };

            var created = await _consultationRepository.AddAsync(consultation);
            return new ConsultationDTO
            {
                Id = created.Id,
                Description = created.Description,
                Date = created.Date,
                PetId = created.PetId,
                VeterinarianId = created.VeterinarianId
            };
        }

        public async Task<ConsultationDTO> UpdateConsultationAsync(int id, ConsultationDTO dto)
        {
            var existing = await _consultationRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new NotFoundException($"Consultation with id {id} was not found.");
            }

            if (await _petRepository.GetByIdAsync(dto.PetId) == null)
            {
                throw new BadRequestException("PetId does not reference an existing pet.");
            }

            if (await _userRepository.GetByIdAsync(dto.VeterinarianId) is not Veterinarian)
            {
                throw new BadRequestException("VeterinarianId does not reference an existing veterinarian.");
            }

            existing.Description = dto.Description;
            existing.Date = dto.Date;
            existing.PetId = dto.PetId;
            existing.VeterinarianId = dto.VeterinarianId;

            var updated = await _consultationRepository.UpdateAsync(id, existing);
            if (updated == null)
            {
                throw new NotFoundException($"Consultation with id {id} was not found.");
            }

            return new ConsultationDTO
            {
                Id = updated.Id,
                Description = updated.Description,
                Date = updated.Date,
                PetId = updated.PetId,
                VeterinarianId = updated.VeterinarianId
            };
        }

        public async Task DeleteConsultationAsync(int id)
        {
            var exists = await _consultationRepository.ExistsAsync(id);
            if (!exists)
            {
                throw new NotFoundException($"Consultation with id {id} was not found.");
            }

            await _consultationRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ConsultationDTO>> GetConsultationsByPetIdAsync(int petId, int? clientId = null)
        {
            if (clientId != null)
            {
                var pet = await _petRepository.GetByIdAsync(petId);
                if (pet == null || pet.ClientId != clientId)
                {
                    throw new NotFoundException($"Pet with id {petId} was not found.");
                }
            }
            var consultations = await _consultationRepository.GetByPetIdAsync(petId);
            var consultationList = consultations.ToList();
            if (consultationList.Count == 0)
            {
                throw new NotFoundException($"No consultations found for pet with id {petId}.");
            }

            return consultationList.Select(consultation => new ConsultationDTO
            {
                Id = consultation.Id,
                Description = consultation.Description,
                Date = consultation.Date,
                PetId = consultation.PetId,
                VeterinarianId = consultation.VeterinarianId
            });
        }

        public async Task<IEnumerable<ConsultationDTO>> GetConsultationsByVeterinarianIdAsync(int veterinarianId)
        {
            var consultations = await _consultationRepository.GetByVeterinarianIdAsync(veterinarianId);
            var consultationList = consultations.ToList();
            if (consultationList.Count == 0)
            {
                throw new NotFoundException($"No consultations found for veterinarian with id {veterinarianId}.");
            }

            return consultationList.Select(consultation => new ConsultationDTO
            {
                Id = consultation.Id,
                Description = consultation.Description,
                Date = consultation.Date,
                PetId = consultation.PetId,
                VeterinarianId = consultation.VeterinarianId
            });
        }
    }
}
