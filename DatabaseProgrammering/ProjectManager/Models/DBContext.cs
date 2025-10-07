using Microsoft.EntityFrameworkCore;

namespace ProjectManager.Models
{
    public class DBContext : DbContext
    {
        public string DbPath { get; }

        public DBContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);

            DbPath = Path.Join(path, "projectmanager.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options.UseSqlite($"Data Source={DbPath}");


        public DbSet<Task> Tasks { get; set; }
        public DbSet<Todo> Todos { get; set; }
    }
}