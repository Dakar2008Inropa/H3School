using System.ComponentModel.DataAnnotations;

namespace GudumholmIF.Models.DTOs.Auth
{
    public sealed class CreateRoleRequestDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}