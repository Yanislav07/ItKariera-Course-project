using Microsoft.AspNetCore.Identity;

namespace Курсов_проект___ИтКариера.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider service)
        {
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = { "Admin", "Moderator", "Author", "User" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
        public static async Task SeedAdminAsync(IServiceProvider service)
        {
            var userManager = service.GetRequiredService<UserManager<User>>();
            var config = service.GetRequiredService<IConfiguration>();

            string adminEmail = config["AdminSettings:Email"];
            string adminPassword = config["AdminSettings:Password"];

            if (string.IsNullOrEmpty(adminEmail)) return;

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
