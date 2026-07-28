using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Client : User
    {
        [Required]
        [StringLength(20)]
        public string Dni { get; set; }

        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}
