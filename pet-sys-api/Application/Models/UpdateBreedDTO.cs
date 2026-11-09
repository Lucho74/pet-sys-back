using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class UpdateBreedDTO
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
