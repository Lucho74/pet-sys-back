using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class CreatePetDTO
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [StringLength(50)]
        public string Specie { get; set; }
        [Required]
        [StringLength(50)]
        public string Breed { get; set; }
        [Required]
        public DateOnly BirthDate { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "ClientId must reference an existing client.")]
        public int ClientId { get; set; }
    }
}
