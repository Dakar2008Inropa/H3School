namespace GudumholmIF.Models.DTOs.Person
{
    public sealed class PersonUpdateDto
    {
        public string CPR { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public int HouseholdId { get; set; }
        public string MembershipState { get; set; }
    }
}