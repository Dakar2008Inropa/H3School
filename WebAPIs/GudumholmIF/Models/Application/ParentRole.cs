namespace GudumholmIF.Models.Application
{
    public sealed class ParentRole
    {
        public int PersonId { get; set; }
        public Person Person { get; set; }
        public int ActiveChildrenCount { get; set; }
    }
}