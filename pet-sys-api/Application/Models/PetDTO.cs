namespace Application.Models
{
    public class PetDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SpeciesId { get; set; }
        public string SpeciesName { get; set; }
        public int BreedId { get; set; }
        public string BreedName { get; set; }
        public DateOnly BirthDate { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public bool IsSuspended { get; set; }
    }
}
