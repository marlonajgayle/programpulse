using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Users;

namespace ProgramPulse.Api.Infrastructure.Persistence;

/// <summary>
/// Registers EF Core persistence, ASP.NET Core Identity, and the database
/// health check. Outside Local/Development/Staging/Production the connection
/// string is read from configuration; in deployed environments it is read from
/// the <c>DATABASE_CONNECTION_STRING</c> environment variable.
/// </summary>
public static class PersistenceConfiguration
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(name: "application_database");

        if (environment.IsDevelopment() || environment.IsStaging() || environment.IsProduction())
        {
            // Use environment variable for connection string
            // in Development, Staging, and Production environments.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString =
                    Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
                    ?? throw new InvalidOperationException(
                        "Environment variable 'DATABASE_CONNECTION_STRING' not found.");

                options.UseSqlServer(
                    connectionString,
                    sql =>
                    {
                        sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        sql.EnableRetryOnFailure(3);
                        sql.CommandTimeout(60);
                    });
            });
        }
        else
        {
            // Local or other environments use appsettings for the connection string.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString =
                    configuration.GetConnectionString("DatabaseConnection")
                    ?? throw new InvalidOperationException(
                        "Connection string 'DatabaseConnection' not found.");

                options.UseSqlServer(
                    connectionString,
                    sql =>
                    {
                        sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        sql.EnableRetryOnFailure();
                        sql.CommandTimeout(60);
                    })
                    .LogTo(Console.WriteLine, LogLevel.Information);
            });
        }

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        return services;
    }
}
