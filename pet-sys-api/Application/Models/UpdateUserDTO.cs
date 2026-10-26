using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class UpdateUserDTO
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [Phone]
        public string Phone { get; set; }
        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }
        [StringLength(20)]
        public string? Dni { get; set; }
    }
}
