using Application.Exceptions;
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
    public class SpeciesServices : ISpeciesServices
    {
        // Toda especie nace con esta raza, así se puede asignar a una mascota sin crear otra raza antes
        private const string DefaultBreedName = "No especificada";

        public readonly ISpeciesRepository _speciesRepository;

        public SpeciesServices(ISpeciesRepository speciesRepository)
        {
            _speciesRepository = speciesRepository;
        }

        public async Task<PaginatedList<SpeciesDTO>> GetSpeciesPagedAsync(PaginationQuery q)
        {
            if (q.PageIndex < 1 || q.PageSize < 1)
            {
                throw new BadRequestException("Page index and page size must be greater than 0.");
            }

            if (q.PageSize > 50)
            {
                throw new BadRequestException("Page size cannot be greater than 50.");
            }

            var page = await _speciesRepository.GetPagedAsync(q);

            var items = page.Items.Select(s => new SpeciesDTO
            {
                Id = s.Id,
                Name = s.Name
            }).ToList();

            return new PaginatedList<SpeciesDTO>(items, page.PageIndex, page.TotalPages);
        }

        public async Task<SpeciesDTO> GetSpeciesByIdAsync(int id)
        {
            var species = await _speciesRepository.GetByIdAsync(id);
            if (species == null)
            {
                throw new NotFoundException($"Species with id {id} was not found.");
            }

            return new SpeciesDTO
            {
                Id = species.Id,
                Name = species.Name
            };
        }

        public async Task<SpeciesDTO> AddSpeciesAsync(CreateEditSpeciesDTO dto)
        {
            var name = dto.Name.Trim();
            if (await _speciesRepository.GetByNameAsync(name) != null)
            {
                throw new BadRequestException($"Species '{name}' already exists.");
            }

            var species = new Species
            {
                Name = name,
                Breeds = new List<Breed> { new Breed { Name = DefaultBreedName } }
            };

            var created = await _speciesRepository.AddAsync(species);
            return new SpeciesDTO
            {
                Id = created.Id,
                Name = created.Name
            };
        }

        public async Task<SpeciesDTO> UpdateSpeciesAsync(int id, CreateEditSpeciesDTO dto)
        {
            var existing = await _speciesRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new NotFoundException($"Species with id {id} was not found.");
            }

            var name = dto.Name.Trim();
            var duplicate = await _speciesRepository.GetByNameAsync(name);
            if (duplicate != null && duplicate.Id != id)
            {
                throw new BadRequestException($"Species '{name}' already exists.");
            }

            existing.Name = name;

            await _speciesRepository.UpdateAsync(id, existing);

            return new SpeciesDTO
            {
                Id = existing.Id,
                Name = existing.Name
            };
        }

        public async Task DeleteSpeciesAsync(int id)
        {
            var species = await _speciesRepository.GetByIdAsync(id);
            if (species == null)
            {
                throw new NotFoundException($"Species with id {id} was not found.");
            }

            // Las mascotas suspendidas también cuentan, para no romper su historia clínica
            if (await _speciesRepository.HasPetsAsync(id))
            {
                throw new BadRequestException($"Species with id {id} has breeds assigned to at least one pet.");
            }

            // Sus razas se borran en cascada
            await _speciesRepository.DeleteAsync(species);
        }
    }
}
