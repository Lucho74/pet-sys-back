using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class UpdateUserRoleDTO
    {
        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;
    }
}
