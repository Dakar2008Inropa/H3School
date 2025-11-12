namespace GudumholmIF.Models.DTOs.Sport
{
    public sealed class SportFeeChangeDto
    {
        public decimal AnnualFeeAdult { get; set; }
        public decimal AnnualFeeChild { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public string Reason { get; set; }
    }
}