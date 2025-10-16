namespace GudumholmIF.Models.Application
{
    public sealed class Sport
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public decimal AnnualFee { get; set; }

        public ICollection<SportFeeHistory> FeeHistory { get; set; } = new List<SportFeeHistory>();
        public ICollection<PersonSport> Members { get; set; } = new List<PersonSport>();
    }
}