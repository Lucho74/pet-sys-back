using Domain.Enums;

namespace Application.Models
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsSuspended { get; set; }
        public UserRole RoleName { get; set; }
        public string? Dni { get; set; }
        public string? RegistrationNumber { get; set; }
    }
}
