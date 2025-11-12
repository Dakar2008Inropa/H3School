namespace GudumholmIF.Models.DTOs.Auth
{
    public sealed class UserWithRolesDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public bool TwoFactorEnabled { get; set; }
        public bool TwoFactorSetupPending { get; set; }
    }
}