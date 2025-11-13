using Microsoft.AspNetCore.Authorization;
using GudumholmIF.Models;
using Microsoft.EntityFrameworkCore;

namespace GudumholmIF.Security
{
    public sealed class UiOrApiKeyHandler : AuthorizationHandler<UiOrApiKeyRequirement>
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<UiOrApiKeyHandler> logger;
        private readonly ClubContext db;

        public UiOrApiKeyHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<UiOrApiKeyHandler> logger,
            ClubContext db
            )
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
            this.db = db;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UiOrApiKeyRequirement requirement)
        {
            HttpContext httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                PathString path = httpContext.Request.Path;
                if (path.StartsWithSegments("/swagger", System.StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWithSegments("/openapi", System.StringComparison.OrdinalIgnoreCase))
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                context.Succeed(requirement);
                return;
            }

            if (httpContext == null)
            {
                return;
            }

            const string HeaderName = "X-API-Key";
            Microsoft.Extensions.Primitives.StringValues headerValues;
            bool hasHeader = httpContext.Request.Headers.TryGetValue(HeaderName, out headerValues);
            if (!hasHeader)
            {
                return;
            }

            string providedKey = headerValues.ToString();
            if (string.IsNullOrWhiteSpace(providedKey))
            {
                return;
            }

            string configuredKey = await db.AppSettings
                .AsNoTracking()
                .Select(s => s.ApiKey)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                logger.LogWarning("API key header provided, but no ApiKey is configured in ApplicationSetting.");
                return;
            }

            if (SecureEquals(providedKey, configuredKey))
            {
                context.Succeed(requirement);
            }
        }

        private static bool SecureEquals(string left, string right)
        {
            if (left.Length != right.Length) return false;
            int diff = 0;
            for (int i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }
            return diff == 0;
        }
    }
}