using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Курсов_проект___ИтКариера.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider service)
        {
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            string[] roles = { "Admin", "Moderator", "Author", "User" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
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

        public static async Task SeedCategoriesAsync(IServiceProvider service)
        {
            var context = service.GetRequiredService<ApplicationDbContext>();

            // Make sure database exists
            await context.Database.MigrateAsync();

            // If categories already exist → stop
            if (await context.Categories.AnyAsync())
                return;

            var categories = new List<Category>
            {
                new Category { Name = "Fiction" },
                new Category { Name = "Mystery" },
                new Category { Name = "Romance" },
                new Category { Name = "Programming" },
                new Category { Name = "Artificial Intelligence" },
                new Category { Name = "Self help" },
                new Category { Name = "UI/UX Design" },
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}
