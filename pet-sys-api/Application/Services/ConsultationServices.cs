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
        public readonly IAzureBlobStorage _azureBlobStorage;

        public ConsultationServices(IConsultationRepository consultationRepository, IPetRepository petRepository, IUserRepository userRepository, IAzureBlobStorage azureBlobStorage)
        {
            _consultationRepository = consultationRepository;
            _petRepository = petRepository;
            _userRepository = userRepository;
            _azureBlobStorage = azureBlobStorage;
        }

        public async Task<ConsultationWithParticipantsDTO> GetConsultationByIdAsync(int id)
        {
            var consultation = await _consultationRepository.GetByIdWithParticipantsAsync(id);
            if (consultation == null)
            {
                throw new KeyNotFoundException($"Consultation with id {id} was not found.");
            }

            return new ConsultationWithParticipantsDTO
            {
                Id = consultation.Id,
                Title = consultation.Title,
                Description = consultation.Description,
                Date = consultation.Date,
                Attachments = consultation.Attachments,
                PetId = consultation.PetId,
                PetName = consultation.Pet.Name,
                VeterinarianId = consultation.VeterinarianId,
                VeterinarianName = consultation.Veterinarian.User.FullName,
                IsSuspended = consultation.IsSuspended
            };
        }

        public async Task<ConsultationDTO> AddConsultationAsync(CreateConsultationDTO dto, int veterinarianId, bool veterinarianOnly = false)
        {
            Pet? pet = await _petRepository.GetByIdAsync(dto.PetId);

            if (pet == null)
            {
                throw new ArgumentException("PetId does not reference an existing pet.");
            }

            if (pet.IsSuspended)
            {
                throw new ArgumentException($"Pet with id {dto.PetId} is suspended.");
            }

            await ValidateResponsibleProfessionalAsync(veterinarianId, veterinarianOnly);

            var consultation = new Consultation
            {
                Title = dto.Title,
                Description = dto.Description,
                Date = dto.Date,
                PetId = dto.PetId,
                VeterinarianId = veterinarianId
            };

            var created = await _consultationRepository.AddAsync(consultation);
            return new ConsultationDTO
            {
                Id = created.Id,
                Title = consultation.Title,
                Description = created.Description,
                Date = created.Date,
                Attachments = created.Attachments,
                PetId = created.PetId,
                VeterinarianId = created.VeterinarianId,
                IsSuspended = created.IsSuspended
            };
        }

        public async Task<ConsultationDTO> UpdateConsultationAsync(int id, UpdateConsultationDTO dto, bool veterinarianOnly = false)
        {
            Consultation? existing = await _consultationRepository.GetByIdAsync(id);

            if (existing == null)
            {
                throw new KeyNotFoundException($"Consultation with id {id} was not found.");
            }

            if (existing.IsSuspended)
            {
                throw new ArgumentException($"Consultation with id {id} is suspended.");
            }

            Pet? pet = await _petRepository.GetByIdAsync(dto.PetId);

            if (pet == null)
            {
                throw new ArgumentException("PetId does not reference an existing pet.");
            }

            if (pet.IsSuspended)
            {
                throw new ArgumentException($"Pet with id {dto.PetId} is suspended.");
            }

            if (dto.VeterinarianId is int veterinarianId && veterinarianId != existing.VeterinarianId)
            {
                await ValidateResponsibleProfessionalAsync(veterinarianId, veterinarianOnly);
                existing.VeterinarianId = veterinarianId;
            }

            existing.Title = dto.Title;
            existing.Description = dto.Description;
            existing.Date = dto.Date;
            existing.PetId = dto.PetId;

            await _consultationRepository.UpdateAsync(id, existing);

            return new ConsultationDTO
            {
                Id = existing.Id,
                Title = existing.Title,
                Description = existing.Description,
                Date = existing.Date,
                Attachments = existing.Attachments,
                PetId = existing.PetId,
                VeterinarianId = existing.VeterinarianId,
                IsSuspended = existing.IsSuspended
            };
        }

        private async Task ValidateResponsibleProfessionalAsync(int veterinarianId, bool veterinarianOnly)
        {
            User? veterinarian = await _userRepository.GetByIdAsync(veterinarianId);

            if (veterinarianOnly && veterinarian?.Role.Name != RoleNames.Veterinarian)
            {
                throw new UnauthorizedAccessException("Admins can only assign consultations to veterinarians or themselves.");
            }

            if (veterinarian == null || (veterinarian.Role.Name != RoleNames.Veterinarian && veterinarian.Role.Name != RoleNames.Admin))
            {
                throw new ArgumentException("VeterinarianId does not reference an existing professional.");
            }

            if (veterinarian.IsSuspended)
            {
                throw new ArgumentException($"Veterinarian with id {veterinarianId} is suspended.");
            }

            if (veterinarian.VeterinarianProfile == null)
            {
                throw new ArgumentException($"Veterinarian with id {veterinarianId} does not have a veterinarian profile.");
            }
        }

        public async Task<IEnumerable<ConsultationDTO>> GetConsultationsByPetIdAsync(int petId)
        {
            var consultations = await _consultationRepository.GetByPetIdAsync(petId);
            return consultations.Select(consultation => new ConsultationDTO
            {
                Id = consultation.Id,
                Title = consultation.Title,
                Description = consultation.Description,
                Date = consultation.Date,
                Attachments = consultation.Attachments,
                PetId = consultation.PetId,
                VeterinarianId = consultation.VeterinarianId,
                IsSuspended = consultation.IsSuspended
            });
        }

        public async Task<IEnumerable<ConsultationDTO>> GetConsultationsByVeterinarianIdAsync(int veterinarianId)
        {
            var consultations = await _consultationRepository.GetByVeterinarianIdAsync(veterinarianId);
            return consultations.Select(consultation => new ConsultationDTO
            {
                Id = consultation.Id,
                Title = consultation.Title,
                Description = consultation.Description,
                Date = consultation.Date,
                Attachments = consultation.Attachments,
                PetId = consultation.PetId,
                VeterinarianId = consultation.VeterinarianId,
                IsSuspended = consultation.IsSuspended
            });
        }

        public async Task SuspendConsultationAsync(int id)
        {
            await SetSuspendedAsync(id, true);
        }

        public async Task ReactivateConsultationAsync(int id)
        {
            await SetSuspendedAsync(id, false);
        }

        private async Task SetSuspendedAsync(int id, bool isSuspended)
        {
            Consultation? existing = await _consultationRepository.GetByIdAsync(id);

            if (existing == null)
            {
                throw new KeyNotFoundException($"Consultation with id {id} was not found.");
            }

            if (!isSuspended)
            {
                Pet? pet = await _petRepository.GetByIdAsync(existing.PetId);

                if (pet == null)
                {
                    throw new ArgumentException("The consultation does not reference an existing pet.");
                }

                if (pet.IsSuspended)
                {
                    throw new ArgumentException("A consultation cannot be reactivated while its pet is suspended.");
                }

                User? client = await _userRepository.GetByIdAsync(pet.ClientId);

                if (client == null || client.Role.Name != RoleNames.Client || client.ClientProfile == null)
                {
                    throw new ArgumentException("The pet does not have a valid client.");
                }

                if (client.IsSuspended)
                {
                    throw new ArgumentException("A consultation cannot be reactivated while its owner is suspended.");
                }
            }

            existing.IsSuspended = isSuspended;
            existing.SuspendedWithOwner = false;

            await _consultationRepository.UpdateAsync(id, existing);
        }

        public async Task<PaginatedList<ConsultationWithParticipantsDTO>> GetConsultationsPagedAsync(PaginationQuery q, int? responsibleUserId = null)
        {
            if (q.PageIndex < 1 || q.PageSize < 1)
            {
                throw new ArgumentException("Page index and page size must be greater than 0.");
            }

            if (q.PageSize > 50)
            {
                throw new ArgumentException("Page size cannot be greater than 50.");
            }

            var page = await _consultationRepository.GetPagedAsync(q, responsibleUserId);

            var items = page.Items.Select(consultation => new ConsultationWithParticipantsDTO
            {
                Id = consultation.Id,
                Title = consultation.Title,
                Description = consultation.Description,
                Date = consultation.Date,
                Attachments = consultation.Attachments,
                PetId = consultation.PetId,
                PetName = consultation.Pet.Name,
                VeterinarianId = consultation.VeterinarianId,
                VeterinarianName = consultation.Veterinarian.User.FullName,
                IsSuspended = consultation.IsSuspended
            }).ToList();

            return new PaginatedList<ConsultationWithParticipantsDTO>(items, page.PageIndex, page.TotalPages);
        }

        public async Task UploadAttachmentAsync(int id, Stream fileStream, string contentType, string fileName, CancellationToken cancellationToken = default)
        {
            var existing = await _consultationRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Consultation with id {id} was not found.");
            }

            if (existing.IsSuspended)
            {
                throw new ArgumentException($"Consultation with id {id} belongs to a suspended client.");
            }

            var originalName = Path.GetFileName(fileName.Replace('\\', '/'));
            var blobName = await _azureBlobStorage.UploadAttachmentAsync(fileStream, contentType, cancellationToken);

            existing.Attachments.Add(new ConsultationAttachment(blobName, string.IsNullOrWhiteSpace(originalName) ? null : originalName));

            var updated = await _consultationRepository.UpdateAsync(id, existing);
            if (updated == null)
            {
                throw new KeyNotFoundException($"Consultation with id {id} was not found.");
            }
        }

        public async Task DeleteAttachmentAsync(int id, string blobName, CancellationToken cancellationToken = default)
        {
            var existing = await _consultationRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Consultation with id {id} was not found.");
            }

            if (existing.IsSuspended)
            {
                throw new ArgumentException($"Consultation with id {id} belongs to a suspended client.");
            }

            var attachment = existing.Attachments.Find(attachment => attachment.BlobName == blobName);
            if (attachment == null)
            {
                return;
            }

            await _azureBlobStorage.DeleteBlobAsync(blobName, cancellationToken);

            existing.Attachments.Remove(attachment);

            var updated = await _consultationRepository.UpdateAsync(id, existing);
            if (updated == null)
            {
                throw new KeyNotFoundException($"Consultation with id {id} was not found.");
            }
        }
    }
}
