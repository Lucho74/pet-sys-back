using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class CreateEditSpeciesDTO
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
