namespace Application.Models
{
    public class DashboardDTO
    {
        public DashboardUserDTO User { get; set; } = new();
        public List<RecentConsultationDTO> RecentConsultations { get; set; } = new();
        public List<RecentPetDTO> RecentPets { get; set; } = new();
    }

    public class DashboardUserDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class RecentConsultationDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool IsSuspended { get; set; }
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int VeterinarianId { get; set; }
        public string VeterinarianName { get; set; } = string.Empty;
    }

    public class RecentPetDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SpeciesName { get; set; } = string.Empty;
        public string BreedName { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public bool IsSuspended { get; set; }
    }
}
