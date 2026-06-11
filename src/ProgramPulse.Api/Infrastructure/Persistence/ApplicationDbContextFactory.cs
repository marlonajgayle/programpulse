using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ProgramPulse.Api.Infrastructure.Authentication;

namespace ProgramPulse.Api.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the EF Core tooling (e.g. <c>dotnet ef migrations</c>).
/// Resolves the connection string from configuration so migrations can be
/// scaffolded without booting the full host.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Local";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("DatabaseConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DatabaseConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(
            connectionString,
            sql =>
            {
                sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(3);
            });

        // No HTTP context exists at design time, so the current user resolves to
        // null and audit stamping falls back to the system principal.
        var currentUser = new CurrentUserService(new HttpContextAccessor());

        return new ApplicationDbContext(optionsBuilder.Options, currentUser);
    }
}
