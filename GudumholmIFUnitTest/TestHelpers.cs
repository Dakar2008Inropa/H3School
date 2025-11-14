using System.Security.Claims;
using FluentAssertions;
using GudumholmIF.Controllers;
using GudumholmIF.Models.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GudumholmIF.Tests
{
    public static class TestHelpers
    {
        public static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            Mock<IUserStore<ApplicationUser>> store = new Mock<IUserStore<ApplicationUser>>();
            Mock<UserManager<ApplicationUser>> userManager =
                new Mock<UserManager<ApplicationUser>>(
                    store.Object, null, null, null, null, null, null, null, null);

            return userManager;
        }

        public static Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock(UserManager<ApplicationUser> userManager)
        {
            Mock<IHttpContextAccessor> contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.SetupGet(a => a.HttpContext).Returns(new DefaultHttpContext());

            Mock<IUserClaimsPrincipalFactory<ApplicationUser>> claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            Mock<SignInManager<ApplicationUser>> signInManager =
                new Mock<SignInManager<ApplicationUser>>(
                    userManager, contextAccessor.Object, claimsFactory.Object, null, null, null, null);

            return signInManager;
        }

        public static RoleManager<IdentityRole> CreateRoleManager(out Mock<IRoleStore<IdentityRole>> storeMock)
        {
            storeMock = new Mock<IRoleStore<IdentityRole>>();

            List<IRoleValidator<IdentityRole>> validators = new List<IRoleValidator<IdentityRole>>
            {
                new RoleValidator<IdentityRole>()
            };

            Mock<ILookupNormalizer> normalizer = new Mock<ILookupNormalizer>();
            normalizer.Setup(n => n.NormalizeName(It.IsAny<string>()))
                      .Returns<string>(s => s?.ToUpperInvariant());

            Mock<ILogger<RoleManager<IdentityRole>>> logger = new Mock<ILogger<RoleManager<IdentityRole>>>();

            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(
                storeMock.Object, validators, normalizer.Object, new IdentityErrorDescriber(), logger.Object);

            return roleManager;
        }

        public static AuthController CreateController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<ApplicationUser> userStore,
            ClaimsPrincipal user = null)
        {
            Mock<ILogger<AuthController>> logger = new Mock<ILogger<AuthController>>();

            AuthController controller = new AuthController(
                userManager,
                signInManager,
                logger.Object,
                roleManager,
                userStore
            );

            DefaultHttpContext httpContext = new DefaultHttpContext();
            if (user != null)
            {
                httpContext.User = user;
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        public static T ReadAnonymousViaJson<T>(object value)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(value);
            T typed = System.Text.Json.JsonSerializer.Deserialize<T>(json);
            typed.Should().NotBeNull("JSON should deserialize correctly");
            return typed!;
        }

        public static ClaimsPrincipal CreatePrincipal(string userId, IEnumerable<Claim> extraClaims = null)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            if (extraClaims != null)
            {
                claims.AddRange(extraClaims);
            }

            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            return principal;
        }
    }
}