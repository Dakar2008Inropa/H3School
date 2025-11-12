using System.ComponentModel.DataAnnotations;

namespace GudumholmIF.Models.DTOs.Auth
{
    public sealed class RegisterRequestDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new List<string>();
    }
}