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
        public bool IsDeleted { get; set; } = false;
        public string RoleName { get; set; }
        [StringLength(20)]
        public string? Dni { get; set; }

    }
}
