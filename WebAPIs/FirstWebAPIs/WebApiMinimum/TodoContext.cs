using Microsoft.EntityFrameworkCore;
using WebApiMinimum.Models;

namespace WebApiMinimum
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options) { }

        public DbSet<Todo> Todos => Set<Todo>();
    }
}