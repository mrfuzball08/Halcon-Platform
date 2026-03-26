using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Backend.Migrations;

public sealed class BackendMigrationsDbContextFactory : IDesignTimeDbContextFactory<BackendMigrationsDbContext>
{
    public BackendMigrationsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("MIGRATIONS_DB_CONNECTION")
            ?? Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<BackendMigrationsDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
        });

        return new BackendMigrationsDbContext(optionsBuilder.Options);
    }
}
