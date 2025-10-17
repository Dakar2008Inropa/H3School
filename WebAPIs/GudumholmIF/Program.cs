using GudumholmIF.Interfaces;
using GudumholmIF.Mapping;
using GudumholmIF.Models;
using GudumholmIF.Services;
using GudumholmIF.Utilites;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GudumholmIF
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ClubContext>
                (opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("ClubDb"),
                sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

            builder.Services.AddScoped<IFeeCalculator, FeeCalculator>();
            builder.Services.AddHostedService<ParentRefreshService>();

            builder.Services.AddControllers().ConfigureApiBehaviorOptions(o => o.SuppressInferBindingSourcesForParameters = true);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();

            builder.Services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new() { Title = "Gudumholm IF API", Version = "v1" });
            });

            TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;
            MapsterConfig.Register(config);

            builder.Services.AddSingleton(config);
            builder.Services.AddScoped<IMapper, ServiceMapper>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gudumholm IF API v1"));
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles(new DefaultFilesOptions { RequestPath = "/ui" });

            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }
    }
}