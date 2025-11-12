namespace GudumholmIF.Models.DTOs.Settings
{
    public sealed class SettingsDto
    {
        public int Id { get; set; }
        public decimal PassiveAdultAnnualFee { get; set; }
        public decimal PassiveChildAnnualFee { get; set; }
    }
}