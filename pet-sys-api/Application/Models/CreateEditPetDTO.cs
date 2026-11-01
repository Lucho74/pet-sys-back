using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class CreateEditPetDTO
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "SpeciesId must be a positive number.")]
        public int SpeciesId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "BreedId must be a positive number.")]
        public int BreedId { get; set; }
        [Required]
        public DateOnly? BirthDate { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "ClientId must be a positive number.")]
        public int ClientId { get; set; }
    }
}
