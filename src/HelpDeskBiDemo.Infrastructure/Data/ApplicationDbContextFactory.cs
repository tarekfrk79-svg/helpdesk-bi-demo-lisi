using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HelpDeskBiDemo.Infrastructure.Data;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = ResolveConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A SQL Server connection string is required for design-time EF Core operations. " +
                "Set ConnectionStrings__DefaultConnection before running dotnet ef.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string? ResolveConnectionString()
    {
        var environmentConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return environmentConnectionString;
        }

        var currentDirectory = Directory.GetCurrentDirectory();
        var candidatePaths = new[]
        {
            currentDirectory,
            Path.Combine(currentDirectory, "..", "HelpDeskBiDemo.Web"),
            Path.Combine(currentDirectory, "src", "HelpDeskBiDemo.Web"),
            Path.Combine(currentDirectory, "..", "..", "src", "HelpDeskBiDemo.Web")
        }
        .Select(Path.GetFullPath)
        .Distinct()
        .ToArray();

        foreach (var path in candidatePaths.Where(Directory.Exists))
        {
            var appSettingsPath = Path.Combine(path, "appsettings.json");
            var developmentAppSettingsPath = Path.Combine(path, "appsettings.Development.json");

            var connectionString = TryReadConnectionString(appSettingsPath)
                ?? TryReadConnectionString(developmentAppSettingsPath);

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }
        }

        return null;
    }

    private static string? TryReadConnectionString(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        using var stream = File.OpenRead(filePath);
        using var document = JsonDocument.Parse(stream);

        if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings))
        {
            return null;
        }

        if (!connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection))
        {
            return null;
        }

        return defaultConnection.ValueKind == JsonValueKind.String
            ? defaultConnection.GetString()
            : null;
    }
}
