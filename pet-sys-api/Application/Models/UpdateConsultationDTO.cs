using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Models
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class UpdateConsultationDTO
    {
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        [Required]
        public DateTime? Date { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "PetId must be a positive number.")]
        public int PetId { get; set; }
    }
}
