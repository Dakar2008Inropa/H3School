using GudumholmIF.Interfaces;
using GudumholmIF.Mapping;
using GudumholmIF.Models;
using GudumholmIF.Services;
using GudumholmIF.Utilites;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using GudumholmIF.Models.Application;
using Microsoft.AspNetCore.Identity;
using GudumholmIF.Infrastructure;
using GudumholmIF.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using System.IO;

namespace GudumholmIF
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string logsPath = Path.Combine(builder.Environment.ContentRootPath, "Logs");
            Directory.CreateDirectory(logsPath);

            builder.Logging.AddConsole();
            builder.Logging.AddLog4Net("log4net.config");

            builder.Services.AddDbContext<ClubContext>
                (opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("ClubDb"),
                sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

            builder.Services.AddScoped<IFeeCalculator, FeeCalculator>();
            builder.Services.AddScoped<IMembershipService, MembershipService>();
            builder.Services.AddHostedService<ParentRefreshService>();

            builder.Services
                .AddIdentityCore<ApplicationUser>(o =>
                {
                    o.Password.RequireDigit = true;
                    o.Password.RequireLowercase = true;
                    o.Password.RequireUppercase = true;
                    o.Password.RequireNonAlphanumeric = true;
                    o.Password.RequiredLength = 10;
                    o.Password.RequiredUniqueChars = 1;

                    o.User.RequireUniqueEmail = true;
                    o.SignIn.RequireConfirmedAccount = false;
                    o.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;

                    o.Lockout.AllowedForNewUsers = false;
                    o.Lockout.MaxFailedAccessAttempts = int.MaxValue;
                    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.Zero;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ClubContext>()
                .AddSignInManager()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme, o =>
            {
                o.LoginPath = "/ui/login.html";
                o.Cookie.Name = "gudumholm.auth";
                o.SlidingExpiration = true;

                o.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        ILogger<Program> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("Unauthorized request to {Path}", context.Request.Path);

                        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync("{\"message\":\"API key required or sign in.\"}");
                        }

                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        ILogger<Program> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("Access denied for {Path}", context.Request.Path);

                        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync("{\"message\":\"Access denied.\"}");
                        }

                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    }
                };
            })
            .AddCookie(IdentityConstants.TwoFactorUserIdScheme)
            .AddCookie(IdentityConstants.ExternalScheme);

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddScoped<IAuthorizationHandler, UiOrApiKeyHandler>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("UiOrApiKey", policy =>
                {
                    policy.Requirements.Add(new UiOrApiKeyRequirement());
                });

                options.FallbackPolicy = options.GetPolicy("UiOrApiKey");
            });

            builder.Services.AddControllers().ConfigureApiBehaviorOptions(o => o.SuppressInferBindingSourcesForParameters = true);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();

            builder.Services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new() { Title = "Gudumholm IF API", Version = "v1" });

                o.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "Provide the API key via X-API-Key header.",
                    Name = "X-API-Key",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                o.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;
            MapsterConfig.Register(config);

            builder.Services.AddSingleton(config);
            builder.Services.AddScoped<IMapper, ServiceMapper>();

            builder.Services.AddTransient<IdentityDataSeeder>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = "application/json";

                        IExceptionHandlerFeature feature = context.Features.Get<IExceptionHandlerFeature>();
                        if (feature != null)
                        {
                            ILogger<Program> logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogError(feature.Error, "Unhandled exception");
                        }

                        await context.Response.WriteAsync("{\"message\":\"An unexpected error occurred.\"}");
                    });
                });
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles(new DefaultFilesOptions { RequestPath = "/ui" });

            app.UseStaticFiles();

            var openAPI = app.MapOpenApi();
            openAPI.AllowAnonymous();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gudumholm IF API v1");
                c.SwaggerEndpoint("/openapi/v1.json", "Gudumholm IF OpenAPI v1");
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            using (IServiceScope scope = app.Services.CreateScope())
            {
                IdentityDataSeeder seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
                try
                {
                    await seeder.SeedAsync();
                }
                catch (Exception ex)
                {
                    app.Logger.LogCritical(ex, "Failed to seed identity data.");
                    throw;
                }
            }

            await app.RunAsync();
        }
    }
}