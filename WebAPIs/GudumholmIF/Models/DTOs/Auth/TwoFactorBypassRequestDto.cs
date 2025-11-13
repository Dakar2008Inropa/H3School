using System.ComponentModel.DataAnnotations;

namespace GudumholmIF.Models.DTOs.Auth
{
    public sealed class TwoFactorBypassRequestDto
    {
        [Required]
        public string MasterPassword { get; set; } = string.Empty;
    }
}