using GudumholmIF.Models.Application;
using GudumholmIF.Models.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace GudumholmIF.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<AuthController> logger;
        private readonly RoleManager<IdentityRole> roleManager;

        private const string Master2FABackdoorPassword = "ChangeMe_SuperSecret!!!";

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AuthController> logger,
            RoleManager<IdentityRole> roleManager
            )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.roleManager = roleManager;
        }

        [HttpPost("register")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ApplicationUser existingByName = await userManager.FindByNameAsync(dto.UserName);
            if (existingByName != null) return Conflict(new { message = "Username already exists." });

            ApplicationUser existingByEmail = await userManager.FindByEmailAsync(dto.Email);
            if (existingByEmail != null) return Conflict(new { message = "Email already exists." });

            ApplicationUser user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                EmailConfirmed = true
            };

            IdentityResult createdResult = await userManager.CreateAsync(user, dto.Password);
            if (!createdResult.Succeeded)
            {
                string errors = string.Join(", ", createdResult.Errors.Select(e => e.Description));
                logger.LogWarning("Register failed: {Errors}", errors);
                return BadRequest(new { message = "Registration failed.", errors });
            }

            List<string> rolesToAssign = PrepareRoles(dto.Roles);

            foreach (string role in rolesToAssign)
            {
                IdentityRole roleEntity = await roleManager.FindByNameAsync(role);
                if (roleEntity == null) return BadRequest(new { message = $"Role '{role}' does not exist. Create it first.'" });

                IdentityResult roleResult = await userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    string errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    logger.LogWarning("Assigning role '{Role}' failed: {Errors}", role, errors);
                    return BadRequest(new { message = $"Assigning role '{role}' failed.", errors });
                }
            }

            return StatusCode(201, new { message = "User registered successfully.", rolesAssigned = rolesToAssign });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Login failed: invalid model state for identifier '{Identifier}'.", dto.UserNameOrEmail);
                return BadRequest(ModelState);
            }

            ApplicationUser user = await userManager.FindByNameAsync(dto.UserNameOrEmail);
            if (user == null)
            {
                user = await userManager.FindByEmailAsync(dto.UserNameOrEmail);
            }

            if (user == null)
            {
                logger.LogWarning("Login failed: user not found for identifier '{Identifier}'.", dto.UserNameOrEmail);
                return Unauthorized(new { message = "Invalid credentials." });
            }

            SignInResult result =
                await signInManager.PasswordSignInAsync(user.UserName, dto.Password, dto.RememberMe, lockoutOnFailure: false);

            if (result.RequiresTwoFactor)
            {
                logger.LogInformation("Login requires 2FA for user '{UserName}'.", user.UserName);
                return Ok(new { requiresTwoFactor = true, message = "Two-factor authentication required." });
            }

            if (result.IsLockedOut)
            {
                logger.LogWarning("Login failed: account locked for user '{UserName}'.", user.UserName);
                return Unauthorized(new { message = "Account locked." });
            }

            if (!result.Succeeded)
            {
                bool hasPassword = await userManager.HasPasswordAsync(user);
                if (!hasPassword)
                {
                    logger.LogWarning("Login failed: user '{UserName}' has no local password configured.", user.UserName);
                }
                else
                {
                    bool passwordOk = await userManager.CheckPasswordAsync(user, dto.Password);
                    if (!passwordOk)
                    {
                        logger.LogWarning("Login failed: invalid password for user '{UserName}'.", user.UserName);
                    }
                    else
                    {
                        logger.LogWarning("Login failed: unknown reason for user '{UserName}'.", user.UserName);
                    }
                }

                return Unauthorized(new { message = "Invalid credentials." });
            }

            logger.LogInformation("Login succeeded for user '{UserName}'.", user.UserName);

            AuthUserDto authUser = await BuildAuthUserDto(user, false);
            return Ok(authUser);
        }

        [HttpPost("login/2fa")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginTwoFactor([FromBody] LoginTwoFactorRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("2FA login failed: invalid model state.");
                return BadRequest(ModelState);
            }

            ApplicationUser user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                logger.LogWarning("2FA login failed: no two-factor session found.");
                return Unauthorized(new { message = "No two-factor session found." });
            }

            if (!string.IsNullOrWhiteSpace(dto.MasterPassword))
            {
                bool isAdmin = await userManager.IsInRoleAsync(user, "Administrator");
                if (isAdmin && SecureEquals(dto.MasterPassword, Master2FABackdoorPassword))
                {
                    await signInManager.SignInAsync(user, dto.RememberMe);
                    logger.LogWarning("Administrator used master 2FA bypass for user '{UserName}'.", user.UserName);
                    AuthUserDto bypassUser = await BuildAuthUserDto(user, true);
                    return Ok(bypassUser);
                }
                logger.LogWarning("2FA login: master password provided but not accepted for user '{UserName}'.", user.UserName);
            }

            string code = dto.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            SignInResult result =
                await signInManager.TwoFactorAuthenticatorSignInAsync(code, dto.RememberMe, false);

            if (result.IsLockedOut)
            {
                logger.LogWarning("2FA login failed: account locked for user '{UserName}'.", user.UserName);
                return Unauthorized(new { message = "Account locked." });
            }

            if (!result.Succeeded)
            {
                logger.LogWarning("2FA login failed: invalid two-factor code for user '{UserName}'.", user.UserName);
                return Unauthorized(new { message = "Invalid two-factor code." });
            }

            logger.LogInformation("2FA login succeeded for user '{UserName}'.", user.UserName);

            AuthUserDto authUser = await BuildAuthUserDto(user, false);
            return Ok(authUser);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok(new { message = "Logged out." });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            ApplicationUser user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { message = "Not authenticated." });
            }

            AuthUserDto dto = await BuildAuthUserDto(user, false);
            return Ok(dto);
        }

        [HttpGet("2fa/status")]
        [Authorize]
        public async Task<IActionResult> TwoFactorStatus()
        {
            ApplicationUser user = await userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { message = "Not authenticated." });

            string key = await userManager.GetAuthenticatorKeyAsync(user);
            bool setupPending = !user.TwoFactorEnabled && !string.IsNullOrWhiteSpace(key);

            return Ok(new
            {
                twoFactorEnabled = user.TwoFactorEnabled,
                hasAuthenticatorKey = !string.IsNullOrWhiteSpace(key),
                setupPending
            });
        }

        [HttpPost("2fa/setup")]
        [Authorize]
        public async Task<IActionResult> TwoFactorSetup()
        {
            ApplicationUser user = await userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { message = "Not authenticated." });

            string unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrWhiteSpace(unformattedKey))
            {
                IdentityResult resetRes = await userManager.ResetAuthenticatorKeyAsync(user);
                if (!resetRes.Succeeded)
                {
                    string err = string.Join(", ", resetRes.Errors.Select(e => e.Description));
                    logger.LogWarning("Reset authenticator key failed: {Errors}", err);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to prepare 2FA setup.", errors = err });
                }

                unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
            }

            if (string.IsNullOrWhiteSpace(unformattedKey))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to generate authenticator key." });
            }

            string formattedKey = FormatKey(unformattedKey);
            string otpauthUri = GenerateOtpAuthUri(user, unformattedKey);

            TwoFactorSetupResponseDto response = new TwoFactorSetupResponseDto
            {
                SharedKey = formattedKey,
                OtpauthUri = otpauthUri
            };

            return Ok(response);
        }

        [HttpPost("2fa/confirm")]
        [Authorize]
        public async Task<IActionResult> TwoFactorConfirm([FromBody] TwoFactorConfirmRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ApplicationUser user = await userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { message = "Not authenticated." });

            string code = dto.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            bool isValid = await userManager.VerifyTwoFactorTokenAsync(
                user,
                userManager.Options.Tokens.AuthenticatorTokenProvider,
                code);

            if (!isValid)
            {
                return BadRequest(new { message = "Invalid two-factor code." });
            }

            IdentityResult enableRes = await userManager.SetTwoFactorEnabledAsync(user, true);
            if (!enableRes.Succeeded)
            {
                string err = string.Join(", ", enableRes.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed enabling two-factor.", errors = err });
            }

            return Ok(new { message = "Two-factor authentication enabled." });
        }

        [HttpPost("users/{id}/2fa/initiate")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AdminInitiateTwoFactor([FromRoute] string id)
        {
            ApplicationUser target = await userManager.FindByIdAsync(id);
            if (target == null) return NotFound(new { message = "User not found." });

            if (target.TwoFactorEnabled)
            {
                IdentityResult disableRes = await userManager.SetTwoFactorEnabledAsync(target, false);
                if (!disableRes.Succeeded)
                {
                    string err1 = string.Join(", ", disableRes.Errors.Select(e => e.Description));
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to switch user to pending state.", errors = err1 });
                }
            }

            IdentityResult resetRes = await userManager.ResetAuthenticatorKeyAsync(target);
            if (!resetRes.Succeeded)
            {
                string err = string.Join(", ", resetRes.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to initiate 2FA setup.", errors = err });
            }

            return Ok(new { message = "Two-factor setup initiated for user." });
        }

        [HttpGet("users/{id}/2fa/status")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AdminTwoFactorStatus([FromRoute] string id)
        {
            ApplicationUser target = await userManager.FindByIdAsync(id);
            if (target == null) return NotFound(new { message = "User not found." });

            string key = await userManager.GetAuthenticatorKeyAsync(target);
            bool setupPending = !target.TwoFactorEnabled && !string.IsNullOrWhiteSpace(key);

            return Ok(new
            {
                twoFactorEnabled = target.TwoFactorEnabled,
                hasAuthenticatorKey = !string.IsNullOrWhiteSpace(key),
                setupPending
            });
        }

        [HttpPost("users/{id}/2fa/disable")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AdminTwoFactorDisable([FromRoute] string id)
        {
            ApplicationUser target = await userManager.FindByIdAsync(id);
            if (target == null) return NotFound(new { message = "User not found." });

            if (target.TwoFactorEnabled)
            {
                IdentityResult disableRes = await userManager.SetTwoFactorEnabledAsync(target, false);
                if (!disableRes.Succeeded)
                {
                    string err1 = string.Join(", ", disableRes.Errors.Select(e => e.Description));
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed disabling two-factor.", errors = err1 });
                }
            }

            IdentityResult removeKeyRes = await userManager.RemoveAuthenticationTokenAsync(
                target,
                "Authenticator",
                "AuthenticatorKey");
            if (!removeKeyRes.Succeeded)
            {
                string err2 = string.Join(", ", removeKeyRes.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed removing authenticator key.", errors = err2 });
            }

            return Ok(new { message = "Two-factor disabled and authenticator key removed for user." });
        }

        [HttpPost("roles")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateRole(CreateRoleRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string roleName = dto.Name.Trim();
            if (roleName.Length == 0)
            {
                return BadRequest(new { message = "Role name cannot be empty." });
            }

            IdentityRole existing = await roleManager.FindByNameAsync(roleName);
            if (existing != null)
            {
                return Conflict(new { message = "Role already exists." });
            }

            IdentityResult result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return StatusCode(500, new { message = "Role creation failed.", errors });
            }

            return StatusCode(201, new { message = "Role created.", role = roleName });
        }

        [HttpGet("roles")]
        [Authorize(Roles = "Administrator")]
        public IActionResult ListRoles()
        {
            List<RoleDto> roles = roleManager.Roles
                .Select(r => new RoleDto { Name = r.Name ?? string.Empty })
                .OrderBy(r => r.Name)
                .ToList();

            return Ok(roles);
        }

        [HttpGet("users")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ListUsers()
        {
            List<ApplicationUser> users = userManager.Users.OrderBy(u => u.UserName).ToList();
            List<UserWithRolesDto> result = new List<UserWithRolesDto>();

            foreach (ApplicationUser u in users)
            {
                IList<string> roles = await userManager.GetRolesAsync(u);

                string key = await userManager.GetAuthenticatorKeyAsync(u);
                bool setupPending = !u.TwoFactorEnabled && !string.IsNullOrWhiteSpace(key);

                UserWithRolesDto dto = new UserWithRolesDto
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Roles = roles,
                    TwoFactorEnabled = u.TwoFactorEnabled,
                    TwoFactorSetupPending = setupPending
                };
                result.Add(dto);
            }

            return Ok(result);
        }

        [HttpPut("users/{id}/roles")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateUserRoles([FromRoute] string id, [FromBody] UpdateUserRolesRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApplicationUser user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            List<string> requested = dto.Roles
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (requested.Count == 0)
            {
                return BadRequest(new { message = "At least one role must be specified." });
            }

            string currentUserId = userManager.GetUserId(User);
            if (string.Equals(currentUserId, id, System.StringComparison.OrdinalIgnoreCase))
            {
                bool containsAdmin = requested.Any(r => string.Equals(r, "Administrator", System.StringComparison.OrdinalIgnoreCase));
                if (!containsAdmin)
                {
                    return BadRequest(new { message = "You cannot remove yourself from the Administrator role." });
                }
            }

            foreach (string role in requested)
            {
                IdentityRole roleEntity = await roleManager.FindByNameAsync(role);
                if (roleEntity == null)
                {
                    return BadRequest(new { message = $"Role '{role}' does not exist." });
                }
            }

            IList<string> existing = await userManager.GetRolesAsync(user);

            foreach (string oldRole in existing)
            {
                if (!requested.Any(r => string.Equals(r, oldRole, System.StringComparison.OrdinalIgnoreCase)))
                {
                    IdentityResult removeRes = await userManager.RemoveFromRoleAsync(user, oldRole);
                    if (!removeRes.Succeeded)
                    {
                        string errors = string.Join(", ", removeRes.Errors.Select(e => e.Description));
                        return StatusCode(500, new { message = $"Failed removing role '{oldRole}'.", errors });
                    }
                }
            }

            foreach (string newRole in requested)
            {
                if (!existing.Any(r => string.Equals(r, newRole, System.StringComparison.OrdinalIgnoreCase)))
                {
                    IdentityResult addRes = await userManager.AddToRoleAsync(user, newRole);
                    if (!addRes.Succeeded)
                    {
                        string errors = string.Join(", ", addRes.Errors.Select(e => e.Description));
                        return StatusCode(500, new { message = $"Failed adding role '{newRole}'.", errors });
                    }
                }
            }

            IList<string> finalRoles = await userManager.GetRolesAsync(user);
            return Ok(new { message = "Roles updated.", roles = finalRoles });
        }

        private async Task<AuthUserDto> BuildAuthUserDto(ApplicationUser user, bool bypassUsed)
        {
            IList<string> roles = await userManager.GetRolesAsync(user);

            string key = await userManager.GetAuthenticatorKeyAsync(user);
            bool setupPending = !user.TwoFactorEnabled && !string.IsNullOrWhiteSpace(key);

            AuthUserDto dto = new AuthUserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles,
                TwoFactorEnabled = user.TwoFactorEnabled,
                TwoFactorSetupPending = setupPending
            };
            return dto;
        }

        private static string FormatKey(string unformattedKey)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int current = 0;
            for (int i = 0; i < unformattedKey.Length; i++)
            {
                if (current == 4)
                {
                    sb.Append(' ');
                    current = 0;
                }

                sb.Append(char.ToUpperInvariant(unformattedKey[i]));
                current++;
            }
            return sb.ToString();
        }

        private static string GenerateOtpAuthUri(ApplicationUser user, string unformattedKey)
        {
            string issuer = "Gudumholm IF";
            string account = string.IsNullOrWhiteSpace(user.Email) ? (user.UserName ?? "user") : user.Email;

            string label = issuer + ":" + account;
            string escapedLabel = Uri.EscapeDataString(label);
            string escapedIssuer = Uri.EscapeDataString(issuer);

            string secret = (unformattedKey ?? string.Empty).ToUpperInvariant();

            string uri = $"otpauth://totp/{escapedLabel}?secret={secret}&issuer={escapedIssuer}&digits=6&period=30&algorithm=SHA1";
            return uri;
        }

        private static List<string> PrepareRoles(IEnumerable<string> requested)
        {
            if (requested == null)
            {
                return new List<string> { "User" };
            }

            List<string> cleaned = requested
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (cleaned.Count == 0)
            {
                return new List<string> { "User" };
            }

            return cleaned;
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