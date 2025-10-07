namespace OwnORM.Models.Views
{
    /// <summary>
    /// Represents dbo.TestView row.
    /// </summary>
    public sealed class TestViewRow
    {
        public string CourseName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int ClassID { get; set; }
        public int Grade { get; set; }
        public string ClassName { get; set; } = string.Empty;
    }
}