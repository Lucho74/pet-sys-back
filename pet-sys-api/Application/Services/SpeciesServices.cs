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
    public class SpeciesServices : ISpeciesServices
    {
        public readonly ISpeciesRepository _speciesRepository;

        public SpeciesServices(ISpeciesRepository speciesRepository)
        {
            _speciesRepository = speciesRepository;
        }

        public async Task<IEnumerable<SpeciesDTO>> GetAllSpeciesAsync()
        {
            var species = await _speciesRepository.GetAllAsync();
            return species.Select(s => new SpeciesDTO
            {
                Id = s.Id,
                Name = s.Name
            });
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
                Name = name
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

            var updated = await _speciesRepository.UpdateAsync(id, existing);
            if (updated == null)
            {
                throw new NotFoundException($"Species with id {id} was not found.");
            }

            return new SpeciesDTO
            {
                Id = updated.Id,
                Name = updated.Name
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
                throw new BadRequestException($"Species with id {id} is assigned to at least one pet.");
            }

            if (await _speciesRepository.HasBreedsAsync(id))
            {
                throw new BadRequestException($"Species with id {id} still has breeds; delete them first.");
            }

            await _speciesRepository.DeleteAsync(species);
        }
    }
}
