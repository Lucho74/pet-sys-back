using Application.Interfaces;
using Application.Models;
using Application.Authorization;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.Services
{
    public class ConsultationServices : IConsultationServices
    {
        public readonly IConsultationRepository _consultationRepository;
        public readonly IPetRepository _petRepository;
        public readonly IUserRepository _userRepository;
        public readonly IAzureBlobStorage _azureBlobStorage;
        private readonly ICurrentUser _currentUser;

        public ConsultationServices(IConsultationRepository consultationRepository, IPetRepository petRepository, IUserRepository userRepository, IAzureBlobStorage azureBlobStorage, ICurrentUser currentUser)
        {
            _consultationRepository = consultationRepository;
            _petRepository = petRepository;
            _userRepository = userRepository;
            _azureBlobStorage = azureBlobStorage;
            _currentUser = currentUser;
        }

        public async Task<ConsultationWithParticipantsDTO> GetConsultationByIdAsync(int id)
        {
            var callerId = _currentUser.Id;
            var callerRole = _currentUser.Role;

            if (!callerId.HasValue || !callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }
            var consultation = await _consultationRepository.GetByIdWithParticipantsAsync(id);
            if (consultation == null || !AccessRules.CanReadPet(callerId.Value, callerRole.Value, consultation.Pet.ClientId))
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

        public async Task<ConsultationDTO> AddConsultationAsync(CreateConsultationDTO dto)
        {
            var callerId = _currentUser.Id;
            var callerRole = _currentUser.Role;

            if (!callerId.HasValue || !callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            if (!AccessRules.IsStaff(callerRole.Value))
            {
                throw new UnauthorizedAccessException("Only staff can perform this operation.");
            }

            if (callerRole.Value == UserRole.SysAdmin && !dto.VeterinarianId.HasValue)
            {
                throw new ArgumentException("SysAdmin must specify the responsible professional.");
            }

            var veterinarianId = dto.VeterinarianId ?? callerId.Value;

            if (!AccessRules.CanCreateConsultationFor(callerId.Value, callerRole.Value, veterinarianId))
            {
                throw new UnauthorizedAccessException("You cannot create a consultation for this professional.");
            }

            bool veterinarianOnly = callerRole.Value == UserRole.Admin && veterinarianId != callerId.Value;
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

        public async Task<ConsultationDTO> UpdateConsultationAsync(int id, UpdateConsultationDTO dto)
        {
            var callerId = _currentUser.Id;
            var callerRole = _currentUser.Role;

            if (!callerId.HasValue || !callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            if (!AccessRules.IsStaff(callerRole.Value))
            {
                throw new UnauthorizedAccessException("Only staff can perform this operation.");
            }

            if (dto.VeterinarianId.HasValue && !AccessRules.CanChangeConsultationProfessional(callerRole.Value))
            {
                throw new UnauthorizedAccessException("You cannot change the responsible professional.");
            }

            bool veterinarianOnly = callerRole.Value == UserRole.Admin && dto.VeterinarianId != callerId.Value;
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

            if (veterinarianOnly && veterinarian?.Role != UserRole.Veterinarian)
            {
                throw new UnauthorizedAccessException("Admins can only assign consultations to veterinarians or themselves.");
            }

            if (veterinarian == null || (veterinarian.Role != UserRole.Veterinarian && veterinarian.Role != UserRole.Admin))
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
            var callerId = _currentUser.Id;
            var callerRole = _currentUser.Role;

            if (!callerId.HasValue || !callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }
            var pet = await _petRepository.GetByIdAsync(petId);

            if (pet == null || !AccessRules.CanReadPet(callerId.Value, callerRole.Value, pet.ClientId))
            {
                throw new KeyNotFoundException($"Pet with id {petId} was not found.");
            }

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
            var callerRole = _currentUser.Role;

            if (!callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            if (!AccessRules.IsStaff(callerRole.Value))
            {
                throw new UnauthorizedAccessException("Only staff can perform this operation.");
            }
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
            var callerRole = _currentUser.Role;

            if (!callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            if (!AccessRules.IsStaff(callerRole.Value))
            {
                throw new UnauthorizedAccessException("Only staff can perform this operation.");
            }
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

                if (client == null || client.Role != UserRole.Client || client.ClientProfile == null)
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

        public async Task<PaginatedList<ConsultationWithParticipantsDTO>> GetConsultationsPagedAsync(PaginationQuery q, bool mine = false)
        {
            var callerId = _currentUser.Id;
            var callerRole = _currentUser.Role;

            if (!callerId.HasValue || !callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            if (!AccessRules.IsStaff(callerRole.Value))
            {
                throw new UnauthorizedAccessException("Only staff can perform this operation.");
            }
            int? responsibleUserId = mine ? callerId.Value : null;

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
            var callerRole = _currentUser.Role;

            if (!callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            if (!AccessRules.IsStaff(callerRole.Value))
            {
                throw new UnauthorizedAccessException("Only staff can perform this operation.");
            }
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
            var callerRole = _currentUser.Role;

            if (!callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            if (!AccessRules.IsStaff(callerRole.Value))
            {
                throw new UnauthorizedAccessException("Only staff can perform this operation.");
            }
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
        public async Task<Uri> GetAttachmentUrlAsync(int id, string blobName, CancellationToken cancellationToken = default)
        {
            var callerId = _currentUser.Id;
            var callerRole = _currentUser.Role;

            if (!callerId.HasValue || !callerRole.HasValue)
            {
                throw new UnauthorizedAccessException("A valid current user is required.");
            }

            var consultation = await _consultationRepository.GetByIdWithParticipantsAsync(id);

            if (consultation == null || !AccessRules.CanReadPet(callerId.Value, callerRole.Value, consultation.Pet.ClientId))
            {
                throw new KeyNotFoundException($"Consultation with id {id} was not found.");
            }

            if (!consultation.Attachments.Any(attachment => attachment.BlobName == blobName))
            {
                throw new KeyNotFoundException("The requested attachment was not found.");
            }

            return await _azureBlobStorage.GetReadUrlAsync(blobName, cancellationToken);
        }
    }
}
