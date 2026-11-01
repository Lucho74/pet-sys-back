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
        [MinLength(1)]
        public List<string> Roles { get; set; } = new();
        [StringLength(8)]
        public string Dni { get; set; } = string.Empty;
        [StringLength(50)]
        public string? RegistrationNumber { get; set; }
    }
}
