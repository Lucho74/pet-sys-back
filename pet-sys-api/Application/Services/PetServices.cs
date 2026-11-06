using Application.Exceptions;
using Application.Interfaces;
using Application.Models;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PetServices : IPetServices
    {
        public readonly IPetRepository _petRepository;
        public readonly IUserRepository _userRepository;
        public readonly IAzureBlobImageStorage _azureBlobImageStorage;

        public PetServices(IPetRepository petRepository, IUserRepository userRepository, IAzureBlobImageStorage azureBlobImageStorage)
        {
            _petRepository = petRepository;
            _userRepository = userRepository;
            _azureBlobImageStorage = azureBlobImageStorage;
        }

        public async Task<IEnumerable<PetDTO>> GetAllPetAsync()
        {
            var pets = await _petRepository.GetAllAsync();
            return pets.Select(pet => new PetDTO
            {
                Id = pet.Id,
                Name = pet.Name,
                Specie = pet.Specie,
                Breed = pet.Breed,
                BlobImage = pet.BlobImage,
                BirthDate = pet.BirthDate,
                ClientId = pet.ClientId,
                IsDeleted = pet.IsDeleted
            });
        }

        public async Task<PetDTO> GetPetByIdAsync(int id)
        {
            var pet = await _petRepository.GetByIdAsync(id);
            if (pet == null)
            {
                throw new NotFoundException($"Pet with id {id} was not found.");
            }

            return new PetDTO
            {
                Id = pet.Id,
                Name = pet.Name,
                Specie = pet.Specie,
                Breed = pet.Breed,
                BlobImage = pet.BlobImage,
                BirthDate = pet.BirthDate,
                ClientId = pet.ClientId,
                IsDeleted = pet.IsDeleted
            };
        }

        public async Task<PetDTO> AddPetAsync(CreateEditPetDTO dto)
        {
            if (await _userRepository.GetByIdAsync(dto.ClientId) is not Client client)
            {
                throw new BadRequestException("ClientId does not reference an existing client.");
            }

            if (client.IsDeleted)
            {
                throw new BadRequestException($"Client with id {dto.ClientId} is suspended.");
            }

            var pet = new Pet
            {
                Name = dto.Name,
                Specie = dto.Specie,
                Breed = dto.Breed,
                BirthDate = dto.BirthDate.Value,
                ClientId = dto.ClientId
            };

            var created = await _petRepository.AddAsync(pet);
            return new PetDTO
            {
                Id = created.Id,
                Name = created.Name,
                Specie = created.Specie,
                Breed = created.Breed,
                BirthDate = created.BirthDate,
                ClientId = created.ClientId,
                IsDeleted = created.IsDeleted
            };
        }

        public async Task<PetDTO> UpdatePetAsync(int id, CreateEditPetDTO dto)
        {
            var existing = await _petRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new NotFoundException($"Pet with id {id} was not found.");
            }

            if (existing.IsDeleted)
            {
                throw new BadRequestException($"Pet with id {id} belongs to a suspended client.");
            }

            if (await _userRepository.GetByIdAsync(dto.ClientId) is not Client client)
            {
                throw new BadRequestException("ClientId does not reference an existing client.");
            }

            if (client.IsDeleted)
            {
                throw new BadRequestException($"Client with id {dto.ClientId} is suspended.");
            }

            existing.Name = dto.Name;
            existing.Specie = dto.Specie;
            existing.Breed = dto.Breed;
            existing.BirthDate = dto.BirthDate.Value;
            existing.ClientId = dto.ClientId;

            var updated = await _petRepository.UpdateAsync(id, existing);
            if (updated == null)
            {
                throw new NotFoundException($"Pet with id {id} was not found.");
            }

            return new PetDTO
            {
                Id = updated.Id,
                Name = updated.Name,
                Specie = updated.Specie,
                Breed = updated.Breed,
                BirthDate = updated.BirthDate,
                ClientId = updated.ClientId,
                IsDeleted = updated.IsDeleted
            };
        }

        public async Task<IEnumerable<PetDTO>> GetPetsByClientAsync(int clientId)
        {
            var pets = await _petRepository.GetByClientIdAsync(clientId);
            return pets.Select(pet => new PetDTO
            {
                Id = pet.Id,
                Name = pet.Name,
                Specie = pet.Specie,
                Breed = pet.Breed,
                BirthDate = pet.BirthDate,
                ClientId = pet.ClientId,
                IsDeleted = pet.IsDeleted
            });
        }

        public async Task<PaginatedList<PetDTO>> GetPetsPagedAsync(PetQuery q)
        {
            if (q.PageIndex < 1 || q.PageSize < 1)
            {
                throw new BadRequestException("Page index and page size must be greater than 0.");
            }

            if (q.PageSize > 50)
            {
                throw new BadRequestException("Page size cannot be greater than 50.");
            }

            var page = await _petRepository.GetPagedAsync(q);

            var items = page.Items.Select(pet => new PetDTO
            {
                Id = pet.Id,
                Name = pet.Name,
                Specie = pet.Specie,
                Breed = pet.Breed,
                BlobImage = pet.BlobImage,
                BirthDate = pet.BirthDate,
                ClientId = pet.ClientId,
                IsDeleted = pet.IsDeleted
            }).ToList();

            return new PaginatedList<PetDTO>(items, page.PageIndex, page.TotalPages);
        }

        public async Task UploadImageAsync(int petId, Stream imageStream, string contentType, CancellationToken cancellationToken = default)
        {
            var existing = await _petRepository.GetByIdAsync(petId);
            if (existing == null)
            {
                throw new NotFoundException($"Pet with id {petId} was not found.");
            }

            if (existing.IsDeleted)
            {
                throw new BadRequestException($"Pet with id {petId} belongs to a suspended client.");
            }

            var blobImage = await _azureBlobImageStorage.UploadAsync(imageStream, contentType, cancellationToken);

            existing.BlobImage = blobImage;

            var updated = await _petRepository.UpdateAsync(petId, existing);
            if (updated == null)
            {
                throw new NotFoundException($"Pet with id {petId} was not found.");
            }
        }

        public async Task DeleteImageAsync(int petId, CancellationToken cancellationToken = default)
        {
            var existing = await _petRepository.GetByIdAsync(petId);
            if (existing == null)
            {
                throw new NotFoundException($"Pet with id {petId} was not found.");
            }

            if (existing.IsDeleted)
            {
                throw new BadRequestException($"Pet with id {petId} belongs to a suspended client.");
            }

            await _azureBlobImageStorage.DeleteAsync(existing.BlobImage, cancellationToken);

            existing.BlobImage = null;

            var updated = await _petRepository.UpdateAsync(petId, existing);
            if (updated == null)
            {
                throw new NotFoundException($"Pet with id {petId} was not found.");
            }
        }
    }
}
