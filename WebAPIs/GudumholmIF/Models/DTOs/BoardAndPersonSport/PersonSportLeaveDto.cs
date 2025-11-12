namespace GudumholmIF.Models.DTOs.BoardAndPersonSport
{
    public sealed class PersonSportLeaveDto
    {
        public DateOnly Left { get; set; }
        public DateTime FullLeftDate { get; set; }
        public bool Active { get; set; }
    }
}