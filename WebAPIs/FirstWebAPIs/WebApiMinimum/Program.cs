using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApiMinimum.Models;

namespace WebApiMinimum
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            var app = builder.Build();

            var todoItems = app.MapGroup("/todoitems");

            todoItems.MapGet("/", GetAllTodos);
            todoItems.MapGet("/complete", GetCompleteTodos);
            todoItems.MapGet("/{id}", GetTodo);
            todoItems.MapPost("/", CreateTodo);
            todoItems.MapPut("/{id}", UpdateTodo);
            todoItems.MapDelete("/{id}", DeleteTodo);

            app.Run();

            static async Task<IResult> GetAllTodos(TodoContext db)
            {
                return TypedResults.Ok(await db.Todos.ToArrayAsync());
            }

            static async Task<IResult> GetCompleteTodos(TodoContext db)
            {
                return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).ToListAsync());
            }

            static async Task<IResult> GetTodo(int id, TodoContext db)
            {
                return await db.Todos.FindAsync(id)
                    is Todo todo
                        ? TypedResults.Ok(todo)
                        : TypedResults.NotFound();
            }

            static async Task<IResult> CreateTodo(Todo todo, TodoContext db)
            {
                db.Todos.Add(todo);
                await db.SaveChangesAsync();

                return TypedResults.Created($"/todoitems/{todo.Id}", todo);
            }

            static async Task<IResult> UpdateTodo(int id, Todo inputTodo, TodoContext db)
            {
                var todo = await db.Todos.FindAsync(id);

                if (todo is null) return TypedResults.NotFound();

                todo.Name = inputTodo.Name;
                todo.IsComplete = inputTodo.IsComplete;

                await db.SaveChangesAsync();

                return TypedResults.NoContent();
            }

            static async Task<IResult> DeleteTodo(int id, TodoContext db)
            {
                if (await db.Todos.FindAsync(id) is Todo todo)
                {
                    db.Todos.Remove(todo);
                    await db.SaveChangesAsync();
                    return TypedResults.NoContent();
                }

                return TypedResults.NotFound();
            }

            app.Run();
        }
    }
}