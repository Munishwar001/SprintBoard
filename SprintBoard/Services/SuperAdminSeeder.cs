using Microsoft.AspNetCore.Identity;
using SprintBoard.Data;

namespace SprintBoard.Services;

public static class SuperAdminSeeder
{
    public static async Task SeedSuperAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var email = configuration["SuperAdmin:Email"]
            ?? throw new InvalidOperationException("SuperAdmin:Email is not configured in user secrets.");
        var password = configuration["SuperAdmin:Password"]
            ?? throw new InvalidOperationException("SuperAdmin:Password is not configured in user secrets.");
        var fullName = configuration["SuperAdmin:FullName"] ?? "Super Admin";

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            // Fix existing SuperAdmin if email was not confirmed
            if (!existing.EmailConfirmed)
            {
                existing.EmailConfirmed = true;
                await userManager.UpdateAsync(existing);
            }
            return;
        }

        var superAdmin = new ApplicationUser
        {
            FullName = fullName,
            Email = email,
            UserName = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(superAdmin, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
    }
}
