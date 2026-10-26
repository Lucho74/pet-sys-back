using Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Consultation : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(500)]
        public string Description { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }
        [Required]
        [ForeignKey(nameof(Pet))]
        public int PetId { get; set; }
        public Pet Pet { get; set; }

        [Required]
        [ForeignKey(nameof(Veterinarian))]
        public int VeterinarianId { get; set; }
        public Veterinarian Veterinarian { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
