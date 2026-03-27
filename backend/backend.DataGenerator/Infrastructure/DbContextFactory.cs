using Backend.Migrations;
using Microsoft.EntityFrameworkCore;

internal static class DbContextFactory
{
    public static BackendMigrationsDbContext Create()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("MIGRATIONS_DB_CONNECTION")
            ?? throw new InvalidOperationException("Environment variable 'MIGRATIONS_DB_CONNECTION' is required.");

        var optionsBuilder = new DbContextOptionsBuilder<BackendMigrationsDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
        });

        return new BackendMigrationsDbContext(optionsBuilder.Options);
    }
}
