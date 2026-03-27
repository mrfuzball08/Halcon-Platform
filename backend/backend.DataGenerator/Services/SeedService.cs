using System.Globalization;
using Backend.Migrations;
using Microsoft.EntityFrameworkCore;

internal static class SeedService
{
    public static async Task RunAsync(BackendMigrationsDbContext db, int ordersToCreate, int profilesToCreate)
    {
        var now = DateTime.UtcNow;
        var random = new Random();

        var customerCatalog = new[]
        {
            ("CUST-100", "Constructora del Norte S.A.", "RFC: CDN010101AAA", "Av. Industria 1234, Monterrey, NL"),
            ("CUST-101", "Materiales y Acabados Express", "RFC: MAE020202BBB", "Blvd. Reforma 567, CDMX"),
            ("CUST-102", "Grupo Constructor Halcon", "RFC: GCH030303CCC", "Av. Chapultepec 890, GDL"),
            ("CUST-103", "Ferreteria El Tornillo Dorado", "RFC: FTD040404DDD", "Calle Morelos 321, Puebla"),
            ("CUST-104", "Aceros Industriales del Bajio", "RFC: AIB050505EEE", "Blvd. Lopez Mateos 1500, Leon"),
            ("CUST-105", "Cementos del Pacifico", "RFC: CDP060606FFF", "Puerto Industrial, Manzanillo, COL")
        };

        var statusCatalog = new[] { "ORDERED", "IN_PROCESS", "IN_ROUTE", "DELIVERED" };

        var orders = new List<OrderEntity>(ordersToCreate);
        var timestamp = now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

        for (var i = 0; i < ordersToCreate; i++)
        {
            var customer = customerCatalog[random.Next(customerCatalog.Length)];
            var status = statusCatalog[random.Next(statusCatalog.Length)];

            var createdAt = now.AddDays(-random.Next(1, 60)).AddMinutes(-random.Next(0, 1440));
            var updatedAt = createdAt.AddHours(random.Next(0, 96));
            if (updatedAt > now)
            {
                updatedAt = now;
            }

            var token = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var invoiceNumber = $"{DataGeneratorConstants.DemoInvoicePrefix}{timestamp}-{i + 1:D5}-{token}";

            var hasLoadingPhoto = status is "IN_ROUTE" or "DELIVERED";
            var hasDeliveryPhoto = status is "DELIVERED";

            orders.Add(new OrderEntity
            {
                InvoiceNumber = invoiceNumber,
                CustomerNumber = customer.Item1,
                CustomerName = customer.Item2,
                FiscalData = $"{customer.Item3}\nDomicilio fiscal: {customer.Item4}",
                DeliveryAddress = $"Entrega demo #{i + 1}, Mexico",
                Notes = $"Demo order generated for showcase. {DataGeneratorConstants.DemoNoteMarker}",
                Status = status,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                IsDeleted = random.NextDouble() < 0.12,
                LoadingPhotoUrl = hasLoadingPhoto ? "/mock/loading-example.svg" : null,
                DeliveryPhotoUrl = hasDeliveryPhoto ? "/mock/delivery-example.svg" : null
            });
        }

        if (orders.Count > 0)
        {
            await db.Orders.AddRangeAsync(orders);
        }

        var insertedProfiles = 0;
        if (profilesToCreate > 0)
        {
            try
            {
                insertedProfiles = await db.Database.ExecuteSqlInterpolatedAsync($"""
                    WITH candidates AS (
                        SELECT au.id, au.email, row_number() OVER (ORDER BY au.created_at DESC) AS rn
                        FROM auth.users au
                        LEFT JOIN public.profiles p ON p.auth_user_id = au.id
                        WHERE p.auth_user_id IS NULL
                        LIMIT {profilesToCreate}
                    )
                    INSERT INTO public.profiles (auth_user_id, username, email, role)
                    SELECT
                        c.id,
                        {DataGeneratorConstants.DemoUsernamePrefix} || substring(replace(c.id::text, '-', '') from 1 for 10),
                        coalesce(nullif(c.email, ''), 'demo+' || substring(replace(c.id::text, '-', '') from 1 for 12) || '@halcon.local'),
                        (ARRAY['ADMIN', 'SALES', 'PURCHASING', 'WAREHOUSE', 'ROUTE'])[1 + ((c.rn - 1) % 5)]
                    FROM candidates c;
                    """);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profile generation skipped: {ex.Message}");
            }
        }

        await db.SaveChangesAsync();

        Console.WriteLine($"Seed complete. Inserted orders: {orders.Count}. Inserted profiles: {insertedProfiles}.");
    }
}
