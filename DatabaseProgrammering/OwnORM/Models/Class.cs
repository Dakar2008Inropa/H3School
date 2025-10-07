namespace OwnORM.Models
{
    /// <summary>
    /// Represents dbo.Class row.
    /// </summary>
    public sealed class Class
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string ClassDescription { get; set; } = string.Empty;
    }
}