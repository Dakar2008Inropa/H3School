using System.ComponentModel.DataAnnotations;

namespace GudumholmIF.Models.DTOs.Auth
{
    public sealed class LoginTwoFactorRequestDto
    {
        [Required]
        public string TwoFactorCode { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        public string MasterPassword { get; set; } = string.Empty;
    }
}