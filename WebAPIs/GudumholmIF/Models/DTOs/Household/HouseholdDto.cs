namespace GudumholmIF.Models.DTOs.Household
{
    public sealed class HouseholdDto
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public int MemberCount { get; set; }
    }
}