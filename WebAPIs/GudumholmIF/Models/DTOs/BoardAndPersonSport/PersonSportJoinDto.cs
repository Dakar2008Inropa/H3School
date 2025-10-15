namespace GudumholmIF.Models.DTOs.BoardAndPersonSport
{
    public sealed class PersonSportJoinDto
    {
        public int SportId { get; set; }
        public DateOnly? Joined { get; set; }
    }
}