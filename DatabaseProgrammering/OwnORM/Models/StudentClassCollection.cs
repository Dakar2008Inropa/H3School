namespace OwnORM.Models
{
    /// <summary>
    /// Represents dbo.Student_Class_Collection row.
    /// </summary>
    public sealed class StudentClassCollection
    {
        public int StudentID { get; set; }
        public int ClassID { get; set; }
        public DateTime StartDate { get; set; }
    }
}