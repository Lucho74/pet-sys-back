using Application.Exceptions;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Interfaces;

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

            await _petRepository.UpdateAsync(id, existing);

            return new PetDTO
            {
                Id = existing.Id,
                Name = existing.Name,
                Specie = existing.Specie,
                Breed = existing.Breed,
                BirthDate = existing.BirthDate,
                ClientId = existing.ClientId,
                IsDeleted = existing.IsDeleted
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
    }
}
