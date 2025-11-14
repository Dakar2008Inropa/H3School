using FluentAssertions;
using GudumholmIF.Controllers;
using GudumholmIF.Models.Application;
using GudumholmIF.Models.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace GudumholmIF.Tests
{
    public sealed class AuthControllerTests
    {
        [Fact]
        public async Task Login_InvalidModel_ReturnsBadRequest()
        {
            Mock<UserManager<ApplicationUser>> um = TestHelpers.CreateUserManagerMock();
            Mock<SignInManager<ApplicationUser>> sm = TestHelpers.CreateSignInManagerMock(um.Object);
            Mock<IUserStore<ApplicationUser>> userStore = new Mock<IUserStore<ApplicationUser>>();
            RoleManager<IdentityRole> rm = TestHelpers.CreateRoleManager(out Mock<IRoleStore<IdentityRole>> _);

            AuthController controller = TestHelpers.CreateController(um.Object, sm.Object, rm, userStore.Object);
            controller.ModelState.AddModelError("UserNameOrEmail", "Required");

            LoginRequestDto dto = new LoginRequestDto
            {
                UserNameOrEmail = string.Empty,
                Password = "x",
                RememberMe = false
            };

            IActionResult result = await controller.Login(dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Login_UserNotFound_ReturnsUnauthorized()
        {
            Mock<UserManager<ApplicationUser>> um = TestHelpers.CreateUserManagerMock();
            um.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);
            um.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            Mock<SignInManager<ApplicationUser>> sm = TestHelpers.CreateSignInManagerMock(um.Object);
            Mock<IUserStore<ApplicationUser>> userStore = new Mock<IUserStore<ApplicationUser>>();
            RoleManager<IdentityRole> rm = TestHelpers.CreateRoleManager(out Mock<IRoleStore<IdentityRole>> _);

            AuthController controller = TestHelpers.CreateController(um.Object, sm.Object, rm, userStore.Object);

            LoginRequestDto dto = new LoginRequestDto { UserNameOrEmail = "unknown@user", Password = "pw", RememberMe = false };

            IActionResult result = await controller.Login(dto);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Login_RequiresTwoFactor_ReturnsOkWithRequiresTwoFactorTrue()
        {
            ApplicationUser user = new ApplicationUser { UserName = "alice", Email = "alice@example.com" };

            Mock<UserManager<ApplicationUser>> um = TestHelpers.CreateUserManagerMock();
            um.Setup(m => m.FindByNameAsync("alice")).ReturnsAsync(user);
            um.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            Mock<SignInManager<ApplicationUser>> sm = TestHelpers.CreateSignInManagerMock(um.Object);
            sm.Setup(m => m.PasswordSignInAsync("alice", "pw", false, false))
              .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.TwoFactorRequired);

            Mock<IUserStore<ApplicationUser>> userStore = new Mock<IUserStore<ApplicationUser>>();
            RoleManager<IdentityRole> rm = TestHelpers.CreateRoleManager(out Mock<IRoleStore<IdentityRole>> _);

            AuthController controller = TestHelpers.CreateController(um.Object, sm.Object, rm, userStore.Object);

            LoginRequestDto dto = new LoginRequestDto { UserNameOrEmail = "alice", Password = "pw", RememberMe = false };

            IActionResult result = await controller.Login(dto);

            OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = TestHelpers.ReadAnonymousViaJson<Dictionary<string, object>>(ok.Value!);
            payload.Should().ContainKey("requiresTwoFactor");
            payload["requiresTwoFactor"].ToString().Should().Be("True");
        }

        [Fact]
        public async Task Login_Success_ReturnsAuthUserDto()
        {
            ApplicationUser user = new ApplicationUser { Id = "u1", UserName = "bob", Email = "bob@example.com" };

            Mock<UserManager<ApplicationUser>> um = TestHelpers.CreateUserManagerMock();
            um.Setup(m => m.FindByNameAsync("bob")).ReturnsAsync(user);
            um.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);
            um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            um.Setup(m => m.GetAuthenticatorKeyAsync(user)).ReturnsAsync(string.Empty);

            Mock<SignInManager<ApplicationUser>> sm = TestHelpers.CreateSignInManagerMock(um.Object);
            sm.Setup(m => m.PasswordSignInAsync("bob", "pw", true, false))
              .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            Mock<IUserStore<ApplicationUser>> userStore = new Mock<IUserStore<ApplicationUser>>();
            RoleManager<IdentityRole> rm = TestHelpers.CreateRoleManager(out Mock<IRoleStore<IdentityRole>> _);

            AuthController controller = TestHelpers.CreateController(um.Object, sm.Object, rm, userStore.Object);

            LoginRequestDto dto = new LoginRequestDto { UserNameOrEmail = "bob", Password = "pw", RememberMe = true };

            IActionResult result = await controller.Login(dto);

            OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task LoginTwoFactor_AdminMasterBypass_SucceedsAndSetsBypassFlag()
        {
            ApplicationUser user = new ApplicationUser { Id = "adm1", UserName = "admin", Email = "a@ex.com" };

            Mock<UserManager<ApplicationUser>> um = TestHelpers.CreateUserManagerMock();
            um.Setup(m => m.IsInRoleAsync(user, "Administrator")).ReturnsAsync(true);
            um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Administrator" });
            um.Setup(m => m.GetAuthenticatorKeyAsync(user)).ReturnsAsync(string.Empty);

            Mock<SignInManager<ApplicationUser>> sm = TestHelpers.CreateSignInManagerMock(um.Object);
            sm.Setup(m => m.GetTwoFactorAuthenticationUserAsync()).ReturnsAsync(user);
            sm.Setup(m => m.SignInWithClaimsAsync(user, It.IsAny<bool>(), It.IsAny<IEnumerable<Claim>>()))
              .Returns(Task.CompletedTask);

            Mock<IUserStore<ApplicationUser>> userStore = new Mock<IUserStore<ApplicationUser>>();
            RoleManager<IdentityRole> rm = TestHelpers.CreateRoleManager(out Mock<IRoleStore<IdentityRole>> _);

            AuthController controller = TestHelpers.CreateController(um.Object, sm.Object, rm, userStore.Object);

            LoginTwoFactorRequestDto dto = new LoginTwoFactorRequestDto
            {
                TwoFactorCode = "ignored",
                RememberMe = true,
                MasterPassword = "ChangeMe_SuperSecret!!!"
            };

            IActionResult result = await controller.LoginTwoFactor(dto);

            OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task TwoFactorStatus_SetupPending_WhenKeyExistsAndNotEnabled()
        {
            ApplicationUser user = new ApplicationUser { Id = "u2", UserName = "eve", Email = "e@example.com", TwoFactorEnabled = false };

            Mock<UserManager<ApplicationUser>> um = TestHelpers.CreateUserManagerMock();
            um.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            um.Setup(m => m.GetAuthenticatorKeyAsync(user)).ReturnsAsync("ABC123");

            Mock<SignInManager<ApplicationUser>> sm = TestHelpers.CreateSignInManagerMock(um.Object);
            Mock<IUserStore<ApplicationUser>> userStore = new Mock<IUserStore<ApplicationUser>>();
            RoleManager<IdentityRole> rm = TestHelpers.CreateRoleManager(out Mock<IRoleStore<IdentityRole>> _);

            ClaimsPrincipal principal = TestHelpers.CreatePrincipal(user.Id);
            AuthController controller = TestHelpers.CreateController(um.Object, sm.Object, rm, userStore.Object, principal);

            IActionResult result = await controller.TwoFactorStatus();

            OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = TestHelpers.ReadAnonymousViaJson<Dictionary<string, object>>(ok.Value!);
            payload["hasAuthenticatorKey"].ToString().Should().Be("True");
            payload["setupPending"].ToString().Should().Be("True");
        }

        [Fact]
        public async Task UpdateUserRoles_CannotRemoveSelfFromAdmin_ReturnsBadRequest()
        {
            string userId = "self";
            ApplicationUser target = new ApplicationUser { Id = userId, UserName = "selfuser" };

            Mock<UserManager<ApplicationUser>> um = TestHelpers.CreateUserManagerMock();
            um.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(target);
            um.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            um.Setup(m => m.GetRolesAsync(target)).ReturnsAsync(new List<string> { "Administrator" });

            Mock<SignInManager<ApplicationUser>> sm = TestHelpers.CreateSignInManagerMock(um.Object);

            Mock<IRoleStore<IdentityRole>> storeMock;
            RoleManager<IdentityRole> rm = TestHelpers.CreateRoleManager(out storeMock);
            storeMock.Setup(s => s.FindByNameAsync("User", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new IdentityRole("User"));

            Mock<IUserStore<ApplicationUser>> userStore = new Mock<IUserStore<ApplicationUser>>();

            ClaimsPrincipal principal = TestHelpers.CreatePrincipal(userId);
            AuthController controller = TestHelpers.CreateController(um.Object, sm.Object, rm, userStore.Object, principal);

            UpdateUserRolesRequestDto dto = new UpdateUserRolesRequestDto
            {
                Roles = new List<string> { "User" }
            };

            IActionResult result = await controller.UpdateUserRoles(userId, dto);

            BadRequestObjectResult bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            string json = System.Text.Json.JsonSerializer.Serialize(bad.Value);
            json.Should().Contain("You cannot remove yourself from the Administrator role.");
        }

        [Fact]
        public async Task Register_NullRoles_AssignsDefaultUserRole()
        {
            ApplicationUser created = null!;

            Mock<UserManager<ApplicationUser>> um = TestHelpers.CreateUserManagerMock();
            um.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);
            um.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);
            um.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "StrongPassw0rd!"))
              .Callback<ApplicationUser, string>((u, _) => created = u)
              .ReturnsAsync(IdentityResult.Success);
            um.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
              .ReturnsAsync(IdentityResult.Success);

            Mock<IRoleStore<IdentityRole>> storeMock;
            RoleManager<IdentityRole> rm = TestHelpers.CreateRoleManager(out storeMock);
            storeMock
                .Setup(s => s.FindByNameAsync(It.Is<string>(n => string.Equals(n, "User", StringComparison.OrdinalIgnoreCase)),
                                              It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IdentityRole("User"));

            Mock<IUserStore<ApplicationUser>> userStore = new Mock<IUserStore<ApplicationUser>>();
            Mock<SignInManager<ApplicationUser>> sm = TestHelpers.CreateSignInManagerMock(um.Object);

            AuthController controller = TestHelpers.CreateController(um.Object, sm.Object, rm, userStore.Object);
            RegisterRequestDto dto = new RegisterRequestDto
            {
                UserName = "newuser",
                Email = "new@user.com",
                Password = "StrongPassw0rd!",
                Roles = null!
            };

            IActionResult result = await controller.Register(dto);

            ObjectResult createdResult = result.Should().BeOfType<ObjectResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            var payload = TestHelpers.ReadAnonymousViaJson<Dictionary<string, object>>(createdResult.Value!);
            payload.Should().ContainKey("rolesAssigned");
            string rolesJson = System.Text.Json.JsonSerializer.Serialize(payload["rolesAssigned"]);
            rolesJson.Should().Contain("User");
        }
    }
}