using Application.Exceptions;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Interfaces;

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
            return consultations.Select(consultation => new ConsultationDTO
            {
                Id = consultation.Id,
                Description = consultation.Description,
                Date = consultation.Date,
                PetId = consultation.PetId,
                VeterinarianId = consultation.VeterinarianId,
                IsDeleted = consultation.IsDeleted
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
                VeterinarianId = consultation.VeterinarianId,
                IsDeleted = consultation.IsDeleted
            };
        }

        public async Task<ConsultationDTO> AddConsultationAsync(CreateEditConsultationDTO dto)
        {
            var pet = await _petRepository.GetByIdAsync(dto.PetId);
            if (pet == null)
            {
                throw new BadRequestException("PetId does not reference an existing pet.");
            }

            if (pet.IsDeleted)
            {
                throw new BadRequestException($"Pet with id {dto.PetId} belongs to a suspended client.");
            }

            if (await _userRepository.GetByIdAsync(dto.VeterinarianId) is not Veterinarian)
            {
                throw new BadRequestException("VeterinarianId does not reference an existing veterinarian.");
            }

            var consultation = new Consultation
            {
                Description = dto.Description,
                Date = dto.Date.Value,
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
                VeterinarianId = created.VeterinarianId,
                IsDeleted = created.IsDeleted
            };
        }

        public async Task<ConsultationDTO> UpdateConsultationAsync(int id, CreateEditConsultationDTO dto)
        {
            var existing = await _consultationRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new NotFoundException($"Consultation with id {id} was not found.");
            }

            if (existing.IsDeleted)
            {
                throw new BadRequestException($"Consultation with id {id} belongs to a suspended client.");
            }

            var pet = await _petRepository.GetByIdAsync(dto.PetId);
            if (pet == null)
            {
                throw new BadRequestException("PetId does not reference an existing pet.");
            }

            if (pet.IsDeleted)
            {
                throw new BadRequestException($"Pet with id {dto.PetId} belongs to a suspended client.");
            }

            if (await _userRepository.GetByIdAsync(dto.VeterinarianId) is not Veterinarian)
            {
                throw new BadRequestException("VeterinarianId does not reference an existing veterinarian.");
            }

            existing.Description = dto.Description;
            existing.Date = dto.Date.Value;
            existing.PetId = dto.PetId;
            existing.VeterinarianId = dto.VeterinarianId;

            await _consultationRepository.UpdateAsync(id, existing);

            return new ConsultationDTO
            {
                Id = existing.Id,
                Description = existing.Description,
                Date = existing.Date,
                PetId = existing.PetId,
                VeterinarianId = existing.VeterinarianId,
                IsDeleted = existing.IsDeleted
            };
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
            return consultations.Select(consultation => new ConsultationDTO
            {
                Id = consultation.Id,
                Description = consultation.Description,
                Date = consultation.Date,
                PetId = consultation.PetId,
                VeterinarianId = consultation.VeterinarianId,
                IsDeleted = consultation.IsDeleted
            });
        }

        public async Task<IEnumerable<ConsultationDTO>> GetConsultationsByVeterinarianIdAsync(int veterinarianId)
        {
            var consultations = await _consultationRepository.GetByVeterinarianIdAsync(veterinarianId);
            return consultations.Select(consultation => new ConsultationDTO
            {
                Id = consultation.Id,
                Description = consultation.Description,
                Date = consultation.Date,
                PetId = consultation.PetId,
                VeterinarianId = consultation.VeterinarianId,
                IsDeleted = consultation.IsDeleted
            });
        }
    }
}
