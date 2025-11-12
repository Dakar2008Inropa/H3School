namespace GudumholmIF.Models.DTOs.Person
{
    public sealed class MembershipHistoryDto
    {
        public string State { get; set; }
        public DateOnly ChangedOn { get; set; }
        public string Reason { get; set; }
    }
}