namespace GudumholmIF.Models.Application
{
    public sealed class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CPR { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public int HouseholdId { get; set; }
        public Household HouseHold { get; set; }

        public MembershipState State { get; set; }

        public ParentRole ParentRole { get; set; }
        public ICollection<BoardRole> BoardRoles { get; set; } = new List<BoardRole>();

        public ICollection<PersonSport> Sports { get; set; } = new List<PersonSport>();
    }
}