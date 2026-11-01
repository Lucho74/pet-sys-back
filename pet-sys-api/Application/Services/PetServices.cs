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
                SpeciesId = pet.SpeciesId,
                SpeciesName = pet.Species.Name,
                BreedId = pet.BreedId,
                BreedName = pet.Breed.Name,
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

            var (species, breed) = await GetSpeciesAndBreedAsync(dto.SpeciesId, dto.BreedId);

            existing.Name = dto.Name;
            existing.SpeciesId = dto.SpeciesId;
            existing.BreedId = dto.BreedId;
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
                SpeciesId = updated.SpeciesId,
                SpeciesName = species.Name,
                BreedId = updated.BreedId,
                BreedName = breed.Name,
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
                SpeciesId = pet.SpeciesId,
                SpeciesName = pet.Species.Name,
                BreedId = pet.BreedId,
                BreedName = pet.Breed.Name,
                BirthDate = pet.BirthDate,
                ClientId = pet.ClientId,
                IsDeleted = pet.IsDeleted
            });
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
