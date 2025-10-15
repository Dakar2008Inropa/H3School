namespace GudumholmIF.Models.Application
{
    public sealed class BoardRole
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; }

        public int SportId { get; set; }
        public Sport Sport { get; set; }

        public DateOnly From { get; set; }
        public DateOnly? To { get; set; }
    }
}