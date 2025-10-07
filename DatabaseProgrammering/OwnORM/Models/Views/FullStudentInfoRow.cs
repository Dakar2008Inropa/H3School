namespace OwnORM.Models.Views
{
    /// <summary>
    /// Represents dbo.FullStudentInfoView row and SP result rows.
    /// </summary>
    public sealed class FullStudentInfoRow
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentAddress { get; set; } = string.Empty;
        public int StudentNumberOfCourses { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
    }
}