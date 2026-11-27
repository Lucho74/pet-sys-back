using Application.Interfaces;
using Application.Models;
using Application.Authorization;
using Domain.Interfaces;

namespace Application.Services
{
    public class DashboardServices : IDashboardServices
    {
        private readonly IUserRepository _userRepository;
        private readonly IPetRepository _petRepository;
        private readonly IConsultationRepository _consultationRepository;
        private readonly ICurrentUser _currentUser;

        public DashboardServices(IUserRepository userRepository, IPetRepository petRepository, IConsultationRepository consultationRepository, ICurrentUser currentUser)
        {
            _userRepository = userRepository;
            _petRepository = petRepository;
            _consultationRepository = consultationRepository;
            _currentUser = currentUser;
        }

        public async Task<DashboardDTO> GetDashboardAsync(int take)
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
            var userId = callerId.Value;

            if (take < 1 || take > 20)
            {
                throw new ArgumentException("Take must be between 1 and 20.");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {userId} was not found.");
            }

            var consultations = await _consultationRepository.GetRecentAsync(take);
            var pets = await _petRepository.GetRecentAsync(take);

            return new DashboardDTO
            {
                User = new DashboardUserDTO
                {
                    Id = user.Id,
                    FullName = user.FullName
                },
                RecentConsultations = consultations.Select(c => new RecentConsultationDTO
                {
                    Id = c.Id,
                    Title = c.Title,
                    Date = c.Date,
                    IsSuspended = c.IsSuspended,
                    PetId = c.PetId,
                    PetName = c.Pet.Name,
                    VeterinarianId = c.VeterinarianId,
                    VeterinarianName = c.Veterinarian.User.FullName
                }).ToList(),
                RecentPets = pets.Select(p => new RecentPetDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    SpeciesName = p.Breed.Species.Name,
                    BreedName = p.Breed.Name,
                    ClientId = p.ClientId,
                    ClientName = p.Client.User.FullName,
                    IsSuspended = p.IsSuspended
                }).ToList()
            };
        }
    }
}
