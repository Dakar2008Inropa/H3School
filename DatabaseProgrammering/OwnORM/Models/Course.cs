namespace OwnORM.Models
{
    /// <summary>
    /// Represents dbo.Course row.
    /// </summary>
    public sealed class Course
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
    }
}