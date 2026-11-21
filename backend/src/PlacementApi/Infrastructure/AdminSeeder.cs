using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlacementApi.Data;
using PlacementApi.Data.Entities;

namespace PlacementApi.Infrastructure;

public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");
        var db = services.GetRequiredService<PlacementDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher<AppUser>>();

        if (await db.AppUsers.AnyAsync(u => u.Role == "Admin"))
            return;

        var email = config["SeedAdmin:Email"];
        var password = config["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("No admin exists and SeedAdmin:Email / SeedAdmin:Password are not set, so none was created.");
            return;
        }

        var admin = new AppUser
        {
            Email = email.Trim(),
            Role = "Admin",
            StudentId = null,
            PasswordHash = string.Empty
        };
        admin.PasswordHash = hasher.HashPassword(admin, password);

        db.AppUsers.Add(admin);
        await db.SaveChangesAsync();
        logger.LogInformation("Created the first admin account: {Email}", admin.Email);
    }
}