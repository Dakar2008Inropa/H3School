namespace GudumholmIF.Models.Application
{
    public sealed class ApplicationSetting
    {
        public int Id { get; set; }
        public decimal PassiveAdultAnnualFee { get; set; }
        public decimal PassiveChildAnnualFee { get; set; }
        public string ApiKey { get; set; } = string.Empty;
    }
}