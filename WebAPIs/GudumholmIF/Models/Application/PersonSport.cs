namespace GudumholmIF.Models.Application
{
    public sealed class PersonSport
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; }
        public int SportId { get; set; }
        public Sport Sport { get; set; }
        public DateOnly Joined { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public DateOnly? Left { get; set; }
        public DateTime FullJoinedDate { get; set; }
        public DateTime? FullLeftDate { get; set; }
        public bool Active { get; set; }
    }
}