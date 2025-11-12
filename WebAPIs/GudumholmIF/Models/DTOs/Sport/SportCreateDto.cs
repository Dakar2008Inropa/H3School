namespace GudumholmIF.Models.DTOs.Sport
{
    public sealed class SportCreateDto
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public decimal AnnualFeeAdult { get; set; }
        public decimal AnnualFeeChild { get; set; }
    }
}