using GudumholmIF.Models.Application;
using Microsoft.AspNetCore.Identity;

namespace GudumholmIF.Infrastructure
{
    public sealed class IdentityDataSeeder
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<IdentityDataSeeder> logger;

        public IdentityDataSeeder(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<IdentityDataSeeder> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task SeedAsync()
        {
            string[] roles = new[] { "Administrator", "Moderator", "User" };

            foreach (string role in roles)
            {
                IdentityRole existing = await roleManager.FindByNameAsync(role);
                if (existing != null) continue;

                IdentityResult created = await roleManager.CreateAsync(new IdentityRole(role));
                if (!created.Succeeded)
                {
                    logger.LogError("Failed creating role '{Role}': {Errors}", role, string.Join(", ", created.Errors));
                }
            }

            string adminEmail = "admin@gudumholm.local";
            string adminUserName = "admin";

            ApplicationUser adminUser = await userManager.FindByNameAsync(adminUserName);
            if (adminUser == null)
            {
                string password = "Password1234***";

                adminUser = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                IdentityResult createResult = await userManager.CreateAsync(adminUser, password);
                if (!createResult.Succeeded)
                {
                    logger.LogError("Failed creating admin user: {Errors}", string.Join(", ", createResult.Errors));
                    return;
                }

                IdentityResult lockoutFlag = await userManager.SetLockoutEnabledAsync(adminUser, false);
                if (!lockoutFlag.Succeeded)
                {
                    logger.LogWarning("Failed disabling lockout for admin: {Errors}", string.Join(", ", lockoutFlag.Errors));
                }
                adminUser.AccessFailedCount = 0;
                adminUser.LockoutEnd = null;
                IdentityResult upd = await userManager.UpdateAsync(adminUser);
                if (!upd.Succeeded)
                {
                    logger.LogWarning("Failed updating admin lockout state: {Errors}", string.Join(", ", upd.Errors));
                }

                IdentityResult roleAssign = await userManager.AddToRoleAsync(adminUser, "Administrator");
                if (!roleAssign.Succeeded)
                {
                    logger.LogError("Failed assigning admin user to role 'Administrator': {Errors}", string.Join(", ", roleAssign.Errors));
                }

                logger.LogInformation("Default administrator user created (username: {UserName})", adminUserName);
            }
            else
            {
                bool isAdmin = await userManager.IsInRoleAsync(adminUser, "Administrator");
                if (!isAdmin)
                {
                    IdentityResult addRes = await userManager.AddToRoleAsync(adminUser, "Administrator");
                    if (!addRes.Succeeded)
                    {
                        logger.LogError("Failed adding Administrator role to existing admin user: {Errors}", string.Join(", ", addRes.Errors));
                    }
                }
            }
        }
    }
}