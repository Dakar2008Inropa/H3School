using System.ComponentModel.DataAnnotations;

namespace GudumholmIF.Models.DTOs.Auth
{
    public sealed class UpdateUserRolesRequestDto
    {
        [Required]
        public List<string> Roles { get; set; } = new List<string>();
    }
}