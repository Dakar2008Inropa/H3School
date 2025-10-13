
using GUIWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GUIWebAPI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<DBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

            builder.Services.AddCors(options => 
            {
                options.AddPolicy("ReactClient", policy => 
                {
                    policy.WithOrigins("http://localhost:5295")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("ReactClient");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}