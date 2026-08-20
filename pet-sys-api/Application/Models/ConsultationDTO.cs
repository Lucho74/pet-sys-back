using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class ConsultationDTO
    {
        public int Id { get; set; }
        [Required]
        [StringLength(500)]
        public string Description { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public StatusConsultation Status { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "PetId must reference an existing pet.")]
        public int PetId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "VeterinarianId must reference an existing veterinarian.")]
        public int VeterinarianId { get; set; }
    }
}
