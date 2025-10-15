namespace GudumholmIF.Models.DTOs.BoardAndPersonSport
{
    public sealed class BoardRoleCreateDto
    {
        public int PersonId { get; set; }
        public int SportId { get; set; }
        public DateOnly From { get; set; }
    }
}