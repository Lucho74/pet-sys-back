using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class CreateEditBreedDTO
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "SpeciesId must be a positive number.")]
        public int SpeciesId { get; set; }
    }
}
