using System.ComponentModel.DataAnnotations;

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
