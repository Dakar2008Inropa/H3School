namespace GudumholmIF.Models.DTOs.BoardAndPersonSport
{
    public sealed class PersonSportJoinDto
    {
        public int SportId { get; set; }
        public DateOnly? Joined { get; set; }
        public DateTime FullJoinedDate { get; set; }
        public bool Active { get; set; }
    }
}