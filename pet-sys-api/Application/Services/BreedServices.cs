using Application.Interfaces;
using Application.Models;
using Domain.Common;
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

        public async Task<PaginatedList<BreedDTO>> GetBreedsPagedAsync(BreedQuery q)
        {
            if (q.PageIndex < 1 || q.PageSize < 1)
            {
                throw new ArgumentException("Page index and page size must be greater than 0.");
            }

            if (q.PageSize > 50)
            {
                throw new ArgumentException("Page size cannot be greater than 50.");
            }

            var page = await _breedRepository.GetPagedAsync(q);

            var items = page.Items.Select(b => new BreedDTO
            {
                Id = b.Id,
                Name = b.Name,
                SpeciesId = b.SpeciesId
            }).ToList();

            return new PaginatedList<BreedDTO>(items, page.PageIndex, page.TotalPages);
        }

        public async Task<IEnumerable<BreedDTO>> GetBreedsBySpeciesIdAsync(int speciesId)
        {
            if (await _speciesRepository.GetByIdAsync(speciesId) == null)
            {
                throw new KeyNotFoundException($"Species with id {speciesId} was not found.");
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
                throw new KeyNotFoundException($"Breed with id {id} was not found.");
            }

            return new BreedDTO
            {
                Id = breed.Id,
                Name = breed.Name,
                SpeciesId = breed.SpeciesId
            };
        }

        public async Task<BreedDTO> AddBreedAsync(CreateBreedDTO dto)
        {
            if (await _speciesRepository.GetByIdAsync(dto.SpeciesId) == null)
            {
                throw new ArgumentException("SpeciesId does not reference an existing species.");
            }

            var name = dto.Name.Trim();
            if (await _breedRepository.GetByNameAsync(dto.SpeciesId, name) != null)
            {
                throw new ArgumentException($"Breed '{name}' already exists for species with id {dto.SpeciesId}.");
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

        public async Task<BreedDTO> UpdateBreedAsync(int id, UpdateBreedDTO dto)
        {
            var existing = await _breedRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Breed with id {id} was not found.");
            }

            // La especie de una raza no se cambia: para corregirla se borra y se crea de nuevo
            var name = dto.Name.Trim();
            var duplicate = await _breedRepository.GetByNameAsync(existing.SpeciesId, name);
            if (duplicate != null && duplicate.Id != id)
            {
                throw new ArgumentException($"Breed '{name}' already exists for species with id {existing.SpeciesId}.");
            }

            existing.Name = name;

            await _breedRepository.UpdateAsync(id, existing);

            return new BreedDTO
            {
                Id = existing.Id,
                Name = existing.Name,
                SpeciesId = existing.SpeciesId
            };
        }

        public async Task DeleteBreedAsync(int id)
        {
            var breed = await _breedRepository.GetByIdAsync(id);
            if (breed == null)
            {
                throw new KeyNotFoundException($"Breed with id {id} was not found.");
            }

            // Las mascotas suspendidas también cuentan, para no romper su historia clínica
            if (await _breedRepository.HasPetsAsync(id))
            {
                throw new ArgumentException($"Breed with id {id} is assigned to at least one pet.");
            }

            // Una especie sin razas no se podría asignar a ninguna mascota; para borrarla entera se borra la especie
            if (await _breedRepository.CountBySpeciesIdAsync(breed.SpeciesId) == 1)
            {
                throw new ArgumentException($"Breed with id {id} is the last breed of its species; delete the species instead.");
            }

            await _breedRepository.DeleteAsync(breed);
        }
    }
}
