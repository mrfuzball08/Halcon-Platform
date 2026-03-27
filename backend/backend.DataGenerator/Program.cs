using Microsoft.EntityFrameworkCore;

var parsed = CliOptions.Parse(args);
if (parsed.ShowHelp)
{
    CliOptions.PrintHelp();
    return;
}

await using var db = DbContextFactory.Create();
await db.Database.OpenConnectionAsync();

switch (parsed.Mode)
{
    case CliMode.Seed:
        await SeedService.RunAsync(db, parsed.Orders, parsed.Profiles);
        break;
    case CliMode.Cleanup:
        await CleanupService.RunAsync(db);
        break;
    default:
        throw new InvalidOperationException("Unknown mode.");
}

return;
