using Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace Application.Models
{
    public class CreateUserDTO
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
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserType UserType { get; set; } = UserType.Client;
        [StringLength(20)]
        public string? Dni { get; set; }
    }
}
