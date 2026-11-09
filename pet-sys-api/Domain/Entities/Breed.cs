using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Breed : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [ForeignKey(nameof(Species))]
        public int SpeciesId { get; set; }
        // Cascade: borrar una especie borra sus razas (el service ya verificó que ninguna esté en uso)
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public Species Species { get; set; }

        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}
