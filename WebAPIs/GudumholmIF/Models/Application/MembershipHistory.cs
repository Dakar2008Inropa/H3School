namespace GudumholmIF.Models.Application
{
    public sealed class MembershipHistory
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; }
        public MembershipActivityState State { get; set; }
        public DateOnly ChangedOn { get; set; }
        public string Reason { get; set; }
    }
}