namespace GudumholmIF.Models.DTOs.Sport
{
    public sealed class SportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public decimal AnnualFeeAdult { get; set; }
        public decimal AnnualFeeChild { get; set; }
    }
}