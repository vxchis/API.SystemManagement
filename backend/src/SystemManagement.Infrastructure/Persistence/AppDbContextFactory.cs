using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SystemManagement.Infrastructure.Authentication;

namespace SystemManagement.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        // Support running from:
        // - backend/
        // - backend/src/SystemManagement.WebApi/
        // - backend/src/SystemManagement.Infrastructure/
        var candidatePaths = new[]
        {
            Path.Combine(basePath, "src", "SystemManagement.WebApi"),
            Path.Combine(basePath, "..", "SystemManagement.WebApi"),
            basePath
        };

        var configBasePath = candidatePaths.FirstOrDefault(Directory.Exists) ?? basePath;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(configBasePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=SystemManagementDb_Attachments;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            sql.EnableRetryOnFailure();
        });

        return new AppDbContext(optionsBuilder.Options, currentUserService: null);
    }
}
