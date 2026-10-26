namespace Domain.Entities
{
    public class Veterinarian : User
    {
        public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    }
}
