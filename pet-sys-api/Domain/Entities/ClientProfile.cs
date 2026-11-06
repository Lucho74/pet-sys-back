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
    [Table("ClientProfiles")]
    public class ClientProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public User User { get; set; } = null!;

        [Required]
        [StringLength(8)]
        public string Dni { get; set; } = string.Empty;

        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}
