namespace Application.Models
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsSuspended { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Dni { get; set; }
        public string? RegistrationNumber { get; set; }
    }
}
