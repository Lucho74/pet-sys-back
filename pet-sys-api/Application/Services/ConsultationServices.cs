using Application.Exceptions;
using Application.Interfaces;
using Application.Models;
using Domain.Constants;
using Domain.Common;
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

        public async Task<ConsultationDTO> AddConsultationAsync(CreateConsultationDTO dto, int veterinarianId)
        {
            Pet? pet = await _petRepository.GetByIdAsync(dto.PetId);

            if (pet == null)
            {
                throw new BadRequestException("PetId does not reference an existing pet.");
            }

            if (pet.IsDeleted)
            {
                throw new BadRequestException($"Pet with id {dto.PetId} is suspended.");
            }

            User? veterinarian = await _userRepository.GetByIdAsync(veterinarianId);

            if (veterinarian == null || (veterinarian.Role.Name != RoleNames.Veterinarian && veterinarian.Role.Name != RoleNames.Admin))
            {
                throw new BadRequestException("VeterinarianId does not reference an existing professional.");
            }

            if (veterinarian.IsDeleted)
            {
                throw new BadRequestException($"Veterinarian with id {veterinarianId} is suspended.");
            }

            if (veterinarian.VeterinarianProfile == null)
            {
                throw new BadRequestException($"Veterinarian with id {veterinarianId} does not have a veterinarian profile.");
            }

            var consultation = new Consultation
            {
                Description = dto.Description,
                Date = dto.Date ?? throw new BadRequestException("Date is required."),
                PetId = dto.PetId,
                VeterinarianId = veterinarianId
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

        public async Task<ConsultationDTO> UpdateConsultationAsync(int id, UpdateConsultationDTO dto)
        {
            Consultation? existing = await _consultationRepository.GetByIdAsync(id);

            if (existing == null)
            {
                throw new NotFoundException($"Consultation with id {id} was not found.");
            }

            if (existing.IsDeleted)
            {
                throw new BadRequestException($"Consultation with id {id} is suspended.");
            }

            Pet? pet = await _petRepository.GetByIdAsync(dto.PetId);

            if (pet == null)
            {
                throw new BadRequestException("PetId does not reference an existing pet.");
            }

            if (pet.IsDeleted)
            {
                throw new BadRequestException($"Pet with id {dto.PetId} is suspended.");
            }

            existing.Description = dto.Description;
            existing.Date = dto.Date ?? throw new BadRequestException("Date is required.");
            existing.PetId = dto.PetId;

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
                VeterinarianId = updated.VeterinarianId,
                IsDeleted = updated.IsDeleted
            };
        }

        public async Task<IEnumerable<ConsultationDTO>> GetConsultationsByPetIdAsync(int petId)
        {
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

        public async Task SuspendConsultationAsync(int id)
        {
            await SetDeletedAsync(id, true);
        }

        public async Task ReactivateConsultationAsync(int id)
        {
            await SetDeletedAsync(id, false);
        }

        private async Task SetDeletedAsync(int id, bool isDeleted)
        {
            Consultation? existing = await _consultationRepository.GetByIdAsync(id);

            if (existing == null)
            {
                throw new NotFoundException($"Consultation with id {id} was not found.");
            }

            if (!isDeleted)
            {
                Pet? pet = await _petRepository.GetByIdAsync(existing.PetId);

                if (pet == null)
                {
                    throw new BadRequestException("The consultation does not reference an existing pet.");
                }

                if (pet.IsDeleted)
                {
                    throw new BadRequestException("A consultation cannot be reactivated while its pet is suspended.");
                }

                User? client = await _userRepository.GetByIdAsync(pet.ClientId);

                if (client == null || client.Role.Name != RoleNames.Client || client.ClientProfile == null)
                {
                    throw new BadRequestException( "The pet does not have a valid client.");
                }

                if (client.IsDeleted)
                {
                    throw new BadRequestException("A consultation cannot be reactivated while its owner is suspended.");
                }
            }

            existing.IsDeleted = isDeleted;

            var updated = await _consultationRepository.UpdateAsync(id, existing);

            if (updated == null)
            {
                throw new NotFoundException($"Consultation with id {id} was not found.");
            }
        }

        public async Task<PaginatedList<ConsultationDTO>> GetConsultationsPagedAsync(PaginationQuery q)
        {
            if (q.PageIndex < 1 || q.PageSize < 1)
            {
                throw new BadRequestException("Page index and page size must be greater than 0.");
            }

            if (q.PageSize > 50)
            {
                throw new BadRequestException("Page size cannot be greater than 50.");
            }

            var page = await _consultationRepository.GetPagedAsync(q);

            var items = page.Items.Select(consultation => new ConsultationDTO
            {
                Id = consultation.Id,
                Description = consultation.Description,
                Date = consultation.Date,
                PetId = consultation.PetId,
                VeterinarianId = consultation.VeterinarianId,
                IsDeleted = consultation.IsDeleted
            }).ToList();

            return new PaginatedList<ConsultationDTO>(items, page.PageIndex, page.TotalPages);
        }
    }
}
