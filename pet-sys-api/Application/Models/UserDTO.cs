using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class UserDTO
    {
        public int Id { get; set; }
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
        public bool IsDeleted { get; set; } = false;
    }
}
