using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class UpdateUserRolesDTO
    {
        [Required]
        [MinLength(1)]
        public List<string> Roles { get; set; } = new();

        [StringLength(8)]
        public string? Dni { get; set; }

        [StringLength(50)]
        public string? RegistrationNumber { get; set; }
    }
}
