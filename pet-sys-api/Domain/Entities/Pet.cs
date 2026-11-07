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
    public class Pet : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
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
        [DataType(DataType.Date)]
        public DateOnly BirthDate { get; set; }
        public int ClientId { get; set; }

        [ForeignKey(nameof(ClientId))]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public ClientProfile Client { get; set; } = null!;

        public bool IsSuspended { get; set; } = false;

        public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    }
}
