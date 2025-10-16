namespace GudumholmIF.Models.Application
{
    public sealed class Household
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }

        public ICollection<Person> Members { get; set; } = new List<Person>();
    }
}