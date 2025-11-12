using System.ComponentModel.DataAnnotations;

namespace GudumholmIF.Models.DTOs.Auth
{
    public sealed class TwoFactorConfirmRequestDto
    {
        [Required]
        public string TwoFactorCode { get; set; } = string.Empty;
    }
}