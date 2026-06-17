using Microsoft.AspNetCore.Identity;

namespace SprintBoard.Services;

public static class RoleSeeder
{
    private static readonly string[] Roles = ["SuperAdmin", "Admin", "User"];

    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
