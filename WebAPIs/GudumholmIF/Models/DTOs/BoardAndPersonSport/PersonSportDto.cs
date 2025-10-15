namespace GudumholmIF.Models.DTOs.BoardAndPersonSport
{
    public sealed class PersonSportDto
    {
        public int PersonId { get; set; }
        public int SportId { get; set; }
        public DateOnly Joined { get; set; }
        public DateOnly? Left { get; set; }
    }
}