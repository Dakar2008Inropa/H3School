
using Microsoft.EntityFrameworkCore;
using SecondWebAPI.Models;

namespace SecondWebAPI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUi(options => 
                {
                    options.DocumentPath = "/openapi/v1.json";
                });
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}