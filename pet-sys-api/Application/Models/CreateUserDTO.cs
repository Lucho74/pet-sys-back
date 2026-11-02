using System.ComponentModel.DataAnnotations;


namespace Application.Models
{
    public class CreateUserDTO
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;
        [StringLength(8)]
        public string? Dni { get; set; }
        [StringLength(50)]
        public string? RegistrationNumber { get; set; }
    }
}
