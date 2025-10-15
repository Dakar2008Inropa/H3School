namespace GudumholmIF.Models.DTOs.BoardAndPersonSport
{
    public sealed class BoardRoleDto
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int SportId { get; set; }
        public DateOnly From { get; set; }
        public DateOnly? To { get; set; }
    }
}