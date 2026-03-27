using Backend.Migrations;
using Microsoft.EntityFrameworkCore;

internal static class CleanupService
{
    public static async Task RunAsync(BackendMigrationsDbContext db)
    {
        var deletedOrders = await db.Orders
            .Where(x => x.InvoiceNumber.StartsWith(DataGeneratorConstants.DemoInvoicePrefix)
                        || x.Notes.Contains(DataGeneratorConstants.DemoNoteMarker))
            .ExecuteDeleteAsync();

        var deletedProfiles = await db.Profiles
            .Where(x => x.Username.StartsWith(DataGeneratorConstants.DemoUsernamePrefix))
            .ExecuteDeleteAsync();

        Console.WriteLine($"Cleanup complete. Deleted orders: {deletedOrders}. Deleted profiles: {deletedProfiles}.");
    }
}
