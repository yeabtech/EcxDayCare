using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Daycare.Infrastructure.Identity;

public static class IdentityDataSeeder
{
    public static async Task SeedIdentityAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = ["Admin", "Staff", "Parent"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException(
                        $"Could not create the {role} role: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}");
            }
        }

        var adminEmail = configuration["BOOTSTRAP_ADMIN_EMAIL"];
        var adminPassword = configuration["BOOTSTRAP_ADMIN_PASSWORD"];
        if (string.IsNullOrWhiteSpace(adminEmail) && string.IsNullOrWhiteSpace(adminPassword))
            return;

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            throw new InvalidOperationException(
                "Set both BOOTSTRAP_ADMIN_EMAIL and BOOTSTRAP_ADMIN_PASSWORD to provision the initial administrator.");

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is not null)
        {
            if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
                throw new InvalidOperationException(
                    "The configured bootstrap administrator email is already assigned to a non-admin account.");

            return;
        }

        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Could not provision the initial administrator: {string.Join("; ", result.Errors.Select(error => error.Description))}");

        var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
        if (!addToRoleResult.Succeeded)
            throw new InvalidOperationException(
                $"Could not assign the initial administrator role: {string.Join("; ", addToRoleResult.Errors.Select(error => error.Description))}");
    }
}
