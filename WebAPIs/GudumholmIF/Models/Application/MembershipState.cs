namespace GudumholmIF.Models.Application
{
    public sealed class MembershipState
    {
        public int PersonId { get; set; }
        public Person Person { get; set; }
        public MembershipActivityState State { get; set; }

        public DateOnly? ActiveSince { get; set; }
        public DateOnly? PassiveSince { get; set; }
    }
}