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
    public class BreedServices : IBreedServices
    {
        public readonly IBreedRepository _breedRepository;
        public readonly ISpeciesRepository _speciesRepository;

        public BreedServices(IBreedRepository breedRepository, ISpeciesRepository speciesRepository)
        {
            _breedRepository = breedRepository;
            _speciesRepository = speciesRepository;
        }

        public async Task<IEnumerable<BreedDTO>> GetAllBreedAsync()
        {
            var breeds = await _breedRepository.GetAllAsync();
            return breeds.Select(b => new BreedDTO
            {
                Id = b.Id,
                Name = b.Name,
                SpeciesId = b.SpeciesId
            });
        }

        public async Task<IEnumerable<BreedDTO>> GetBreedsBySpeciesIdAsync(int speciesId)
        {
            if (await _speciesRepository.GetByIdAsync(speciesId) == null)
            {
                throw new NotFoundException($"Species with id {speciesId} was not found.");
            }

            var breeds = await _breedRepository.GetBySpeciesIdAsync(speciesId);
            return breeds.Select(b => new BreedDTO
            {
                Id = b.Id,
                Name = b.Name,
                SpeciesId = b.SpeciesId
            });
        }

        public async Task<BreedDTO> GetBreedByIdAsync(int id)
        {
            var breed = await _breedRepository.GetByIdAsync(id);
            if (breed == null)
            {
                throw new NotFoundException($"Breed with id {id} was not found.");
            }

            return new BreedDTO
            {
                Id = breed.Id,
                Name = breed.Name,
                SpeciesId = breed.SpeciesId
            };
        }

        public async Task<BreedDTO> AddBreedAsync(CreateEditBreedDTO dto)
        {
            if (await _speciesRepository.GetByIdAsync(dto.SpeciesId) == null)
            {
                throw new BadRequestException("SpeciesId does not reference an existing species.");
            }

            var name = dto.Name.Trim();
            if (await _breedRepository.GetByNameAsync(dto.SpeciesId, name) != null)
            {
                throw new BadRequestException($"Breed '{name}' already exists for species with id {dto.SpeciesId}.");
            }

            var breed = new Breed
            {
                Name = name,
                SpeciesId = dto.SpeciesId
            };

            var created = await _breedRepository.AddAsync(breed);
            return new BreedDTO
            {
                Id = created.Id,
                Name = created.Name,
                SpeciesId = created.SpeciesId
            };
        }

        public async Task<BreedDTO> UpdateBreedAsync(int id, CreateEditBreedDTO dto)
        {
            var existing = await _breedRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new NotFoundException($"Breed with id {id} was not found.");
            }

            if (await _speciesRepository.GetByIdAsync(dto.SpeciesId) == null)
            {
                throw new BadRequestException("SpeciesId does not reference an existing species.");
            }

            // Cambiarle la especie a una raza en uso dejaría mascotas con raza y especie inconsistentes
            if (existing.SpeciesId != dto.SpeciesId && await _breedRepository.HasPetsAsync(id))
            {
                throw new BadRequestException($"Breed with id {id} is assigned to at least one pet; its species can't be changed.");
            }

            var name = dto.Name.Trim();
            var duplicate = await _breedRepository.GetByNameAsync(dto.SpeciesId, name);
            if (duplicate != null && duplicate.Id != id)
            {
                throw new BadRequestException($"Breed '{name}' already exists for species with id {dto.SpeciesId}.");
            }

            existing.Name = name;
            existing.SpeciesId = dto.SpeciesId;

            var updated = await _breedRepository.UpdateAsync(id, existing);
            if (updated == null)
            {
                throw new NotFoundException($"Breed with id {id} was not found.");
            }

            return new BreedDTO
            {
                Id = updated.Id,
                Name = updated.Name,
                SpeciesId = updated.SpeciesId
            };
        }

        public async Task DeleteBreedAsync(int id)
        {
            var breed = await _breedRepository.GetByIdAsync(id);
            if (breed == null)
            {
                throw new NotFoundException($"Breed with id {id} was not found.");
            }

            // Las mascotas suspendidas también cuentan, para no romper su historia clínica
            if (await _breedRepository.HasPetsAsync(id))
            {
                throw new BadRequestException($"Breed with id {id} is assigned to at least one pet.");
            }

            await _breedRepository.DeleteAsync(breed);
        }
    }
}
