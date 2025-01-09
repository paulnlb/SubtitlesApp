using Microsoft.AspNetCore.Identity;
using SubtitlesServer.IdentityApi.Models;

namespace SubtitlesServer.IdentityApi.Data;

public static class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<SubAppUser>>();

        // Seed Roles
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Seed Users
        var adminUser = new SubAppUser { UserName = "admin", Email = "admin@example.com", Name = "Paulus" };
        if (userManager.Users.All(u => u.UserName != adminUser.UserName))
        {
            await userManager.CreateAsync(adminUser, "AdminPass123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

}
