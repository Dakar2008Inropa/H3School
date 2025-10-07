namespace OwnORM.Models.Views
{
    /// <summary>
    /// Represents dbo.StudentClassView row.
    /// </summary>
    public sealed class StudentClassViewRow
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentAddress { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
    }
}