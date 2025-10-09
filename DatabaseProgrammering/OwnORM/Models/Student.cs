namespace OwnORM.Models
{
    /// <summary>
    /// Represents dbo.Student row.
    /// </summary>
    public class Student
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentAddress { get; set; } = string.Empty;
        public int ClassID { get; set; }
        public int StudentNumberOfCourses { get; set; }
        public int StudentSumOfAllCharacters { get; set; }

        public StudentType StudentType { get; set; } = StudentType.EUD;
    }
}