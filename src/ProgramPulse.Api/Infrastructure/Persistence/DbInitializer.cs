using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Authorization;

namespace ProgramPulse.Api.Infrastructure.Persistence;

/// <summary>
/// Applies pending migrations and seeds essential reference data on startup.
/// </summary>
public static class DbInitializer
{
    public static async Task<IApplicationBuilder> UseInitializeDatabaseAsync(
        this IApplicationBuilder application)
    {
        using var serviceScope = application.ApplicationServices.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

        // Only migrate when there are pending migrations to apply.
        if (dbContext != null && dbContext.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Initializing Database: Applying Pending Database Migrations...");
            dbContext.Database.Migrate();
        }

        // TODO: Add method(s) for seeding other essential data
        // (e.g. a default admin user) once those features land.

        // Seed default Identity roles.
        var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.GetAllRoles())
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        return application;
    }
}
