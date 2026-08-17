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

        public PetServices(IPetRepository petRepository, IUserRepository userRepository)
        {
            _petRepository = petRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<PetDTO>> GetAllPetAsync()
        {
            var pets = await _petRepository.GetAllAsync();
            var petList = pets.ToList();
            if (petList.Count == 0)
            {
                throw new NotFoundException("No pets found.");
            }

            return petList.Select(pet => new PetDTO
            {
                Id = pet.Id,
                Name = pet.Name,
                Specie = pet.Specie,
                Breed = pet.Breed,
                BirthDate = pet.BirthDate,
                ClientId = pet.ClientId
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
                BirthDate = pet.BirthDate,
                ClientId = pet.ClientId
            };
        }

        public async Task<PetDTO> AddPetAsync(CreatePetDTO dto)
        {
            if (await _userRepository.GetByIdAsync(dto.ClientId) is not Client)
            {
                throw new BadRequestException("ClientId does not reference an existing client.");
            }

            var pet = new Pet
            {
                Name = dto.Name,
                Specie = dto.Specie,
                Breed = dto.Breed,
                BirthDate = dto.BirthDate,
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
                ClientId = created.ClientId
            };
        }

        public async Task<PetDTO> UpdatePetAsync(int id, PetDTO dto)
        {
            var existing = await _petRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new NotFoundException($"Pet with id {id} was not found.");
            }

            if (await _userRepository.GetByIdAsync(dto.ClientId) is not Client)
            {
                throw new BadRequestException("ClientId does not reference an existing client.");
            }

            existing.Name = dto.Name;
            existing.Specie = dto.Specie;
            existing.Breed = dto.Breed;
            existing.BirthDate = dto.BirthDate;
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
                ClientId = updated.ClientId
            };
        }

        public async Task DeletePetAsync(int id)
        {
            var exists = await _petRepository.ExistsAsync(id);
            if (!exists)
            {
                throw new NotFoundException($"Pet with id {id} was not found.");
            }

            await _petRepository.DeleteAsync(id);
        }
    }
}
