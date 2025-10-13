using Microsoft.EntityFrameworkCore;

namespace GUIWebAPI.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity => 
            {
                entity.HasKey(e => e.CategoryId);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
            });

            modelBuilder.Entity<Product>(entity => 
            {
                entity.HasKey(e => e.ProductId);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(450);

                entity.Property(e => e.Price).HasPrecision(18, 2);

                entity.HasOne(e => e.Category).WithMany(c => c.Products).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}