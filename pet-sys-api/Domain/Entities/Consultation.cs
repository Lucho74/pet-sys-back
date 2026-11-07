using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int VeterinarianId { get; set; }
        [ForeignKey(nameof(VeterinarianId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public VeterinarianProfile Veterinarian { get; set; } = null!;
        public bool IsSuspended { get; set; } = false;
    }
}
