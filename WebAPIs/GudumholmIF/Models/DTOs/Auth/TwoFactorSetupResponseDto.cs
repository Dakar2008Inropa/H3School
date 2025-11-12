namespace GudumholmIF.Models.DTOs.Auth
{
    public sealed class TwoFactorSetupResponseDto
    {
        public string SharedKey { get; set; } = string.Empty;
        public string OtpauthUri { get; set; } = string.Empty;
    }
}