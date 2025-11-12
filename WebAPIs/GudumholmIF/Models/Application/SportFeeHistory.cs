namespace GudumholmIF.Models.Application
{
    public sealed class SportFeeHistory
    {
        public int Id { get; set; }
        public int SportId { get; set; }
        public Sport Sport { get; set; }
        public decimal AnnualFeeAdult { get; set; }
        public decimal AnnualFeeChild { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public string Reason { get; set; }
    }
}