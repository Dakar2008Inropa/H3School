namespace OwnORM.Models
{
    /// <summary>
    /// Represents dbo.StudentClass_RepetitionOnClass row.
    /// </summary>
    public sealed class StudentClassRepetitionOnClass
    {
        public int StudentClassID { get; set; }
        public int StudentID { get; set; }
        public int ClassID { get; set; }
        public int Grade { get; set; }
        public int CourseID { get; set; }
    }
}