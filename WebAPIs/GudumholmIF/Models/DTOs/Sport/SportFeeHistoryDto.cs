namespace GudumholmIF.Models.DTOs.Sport
{
    public sealed class SportFeeHistoryDto
    {
        public int Id { get; set; }
        public int SportId { get; set; }
        public decimal AnnualFee { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public string Reason { get; set; }
    }
}