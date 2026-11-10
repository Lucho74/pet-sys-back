using Domain.Entities;

namespace Application.Models
{
    public class ConsultationDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public List<ConsultationAttachment> Attachments { get; set; } = new();
        public int PetId { get; set; }
        public int VeterinarianId { get; set; }
        public bool IsSuspended { get; set; }
    }
}
