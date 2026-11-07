using Application.Exceptions;
using Application.Interfaces;
using Application.Models;
using Domain.Common;
using Domain.Constants;
using Domain.Entities;
using Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PetServices : IPetServices
    {
        public readonly IPetRepository _petRepository;
        public readonly IUserRepository _userRepository;
        public readonly ISpeciesRepository _speciesRepository;
        public readonly IBreedRepository _breedRepository;

        public PetServices(IPetRepository petRepository, IUserRepository userRepository, ISpeciesRepository speciesRepository, IBreedRepository breedRepository)
        {
            _petRepository = petRepository;
            _userRepository = userRepository;
            _speciesRepository = speciesRepository;
            _breedRepository = breedRepository;
        }

        public async Task<IEnumerable<PetDTO>> GetAllPetAsync()
        {
            var pets = await _petRepository.GetAllAsync();
            return pets.Select(pet => new PetDTO
            {
                Id = pet.Id,
                Name = pet.Name,
                SpeciesId = pet.SpeciesId,
                SpeciesName = pet.Species.Name,
                BreedId = pet.BreedId,
                BreedName = pet.Breed.Name,
                BirthDate = pet.BirthDate,
                ClientId = pet.ClientId,
                IsSuspended = pet.IsSuspended
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
                SpeciesId = pet.SpeciesId,
                SpeciesName = pet.Species.Name,
                BreedId = pet.BreedId,
                BreedName = pet.Breed.Name,
                BirthDate = pet.BirthDate,
                ClientId = pet.ClientId,
                IsSuspended = pet.IsSuspended
            };
        }

        public async Task<PetDTO> AddPetAsync(CreateEditPetDTO dto)
        {
            User? client = await _userRepository.GetByIdAsync(dto.ClientId);

            if (client == null || client.Role.Name != RoleNames.Client)
            {
                throw new BadRequestException("ClientId does not reference an existing client.");
            }

            if (client.IsSuspended)
            {
                throw new BadRequestException($"Client with id {dto.ClientId} is suspended.");
            }

            if (client.ClientProfile == null)
            {
                throw new BadRequestException($"Client with id {dto.ClientId} does not have a client profile.");
            }

            var (species, breed) = await GetSpeciesAndBreedAsync(dto.SpeciesId, dto.BreedId);

            var pet = new Pet
            {
                Name = dto.Name,
                SpeciesId = dto.SpeciesId,
                BreedId = dto.BreedId,
                BirthDate = dto.BirthDate.Value,
                ClientId = dto.ClientId
            };

            var created = await _petRepository.AddAsync(pet);
            return new PetDTO
            {
                Id = created.Id,
                Name = created.Name,
                SpeciesId = created.SpeciesId,
                SpeciesName = species.Name,
                BreedId = created.BreedId,
                BreedName = breed.Name,
                BirthDate = created.BirthDate,
                ClientId = created.ClientId,
                IsSuspended = created.IsSuspended
            };
        }

        public async Task<PetDTO> UpdatePetAsync(int id, CreateEditPetDTO dto)
        {
            Pet? existing = await _petRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new NotFoundException($"Pet with id {id} was not found.");
            }

            if (existing.IsSuspended)
            {
                throw new BadRequestException($"Pet with id {id} is suspended.");
            }

            User? client = await _userRepository.GetByIdAsync(dto.ClientId);

            if (client == null || client.Role.Name != RoleNames.Client)
            {
                throw new BadRequestException("ClientId does not reference an existing client.");
            }

            if (client.IsSuspended)
            {
                throw new BadRequestException($"Client with id {dto.ClientId} is suspended.");
            }

            if (client.ClientProfile == null)
            {
                throw new BadRequestException($"Client with id {dto.ClientId} does not have a client profile.");
            }

            var (species, breed) = await GetSpeciesAndBreedAsync(dto.SpeciesId, dto.BreedId);

            existing.Name = dto.Name;
            existing.SpeciesId = dto.SpeciesId;
            existing.BreedId = dto.BreedId;
            existing.BirthDate = dto.BirthDate.Value;
            existing.ClientId = dto.ClientId;

            await _petRepository.UpdateAsync(id, existing);

            return new PetDTO
            {
                Id = existing.Id,
                Name = existing.Name,
                SpeciesId = existing.SpeciesId,
                SpeciesName = species.Name,
                BreedId = existing.BreedId,
                BreedName = breed.Name,
                BirthDate = existing.BirthDate,
                ClientId = existing.ClientId,
                IsSuspended = existing.IsSuspended
            };
        }

        public async Task<IEnumerable<PetDTO>> GetPetsByClientAsync(int clientId)
        {
            var pets = await _petRepository.GetByClientIdAsync(clientId);
            return pets.Select(pet => new PetDTO
            {
                Id = pet.Id,
                Name = pet.Name,
                SpeciesId = pet.SpeciesId,
                SpeciesName = pet.Species.Name,
                BreedId = pet.BreedId,
                BreedName = pet.Breed.Name,
                BirthDate = pet.BirthDate,
                ClientId = pet.ClientId,
                IsSuspended = pet.IsSuspended
            });
        }
        public async Task SuspendPetAsync(int id)
        {
            await SetSuspendedAsync(id, true);
        }

        public async Task ReactivatePetAsync(int id)
        {
            await SetSuspendedAsync(id, false);
        }

        private async Task SetSuspendedAsync(int id, bool isSuspended)
        {
            Pet? existing = await _petRepository.GetByIdAsync(id);

            if (existing == null)
            {
                throw new NotFoundException( $"Pet with id {id} was not found.");
            }

            if (!isSuspended)
            {
                User? client =await _userRepository.GetByIdAsync(existing.ClientId);

                if (client == null || client.Role.Name != RoleNames.Client || client.ClientProfile == null)
                {
                    throw new BadRequestException("The pet does not have a valid client.");
                }

                if (client.IsSuspended)
                {
                    throw new BadRequestException("A pet cannot be reactivated while its owner is suspended.");
                }
            }

            existing.IsSuspended = isSuspended;
            existing.SuspendedWithOwner = false;

            await _petRepository.UpdateAsync(id, existing);
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
                SpeciesId = pet.SpeciesId,
                SpeciesName = pet.Species.Name,
                BreedId = pet.BreedId,
                BreedName = pet.Breed.Name,
                BirthDate = pet.BirthDate,
                ClientId = pet.ClientId,
                IsSuspended = pet.IsSuspended
            }).ToList();

            return new PaginatedList<PetDTO>(items, page.PageIndex, page.TotalPages);
        }

        private async Task<(Species, Breed)> GetSpeciesAndBreedAsync(int speciesId, int breedId)
        {
            var species = await _speciesRepository.GetByIdAsync(speciesId);
            if (species == null)
            {
                throw new BadRequestException("SpeciesId does not reference an existing species.");
            }

            var breed = await _breedRepository.GetByIdAsync(breedId);
            if (breed == null)
            {
                throw new BadRequestException("BreedId does not reference an existing breed.");
            }

            if (breed.SpeciesId != speciesId)
            {
                throw new BadRequestException($"Breed with id {breedId} does not belong to species with id {speciesId}.");
            }

            return (species, breed);
        }
    }
}
