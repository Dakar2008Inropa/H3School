namespace GudumholmIF.Models.DTOs.Sport
{
    public sealed class SportFeeChangeDto
    {
        public decimal AnnualFee { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public string Reason { get; set; }
    }
}