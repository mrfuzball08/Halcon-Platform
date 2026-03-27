internal sealed class CliOptions
{
    public CliMode Mode { get; set; } = CliMode.Seed;
    public int Orders { get; set; } = 250;
    public int Profiles { get; set; } = 25;
    public bool ShowHelp { get; set; }

    public static CliOptions Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return new CliOptions();
        }

        if (args.Any(x => x is "--help" or "-h"))
        {
            return new CliOptions { ShowHelp = true };
        }

        var mode = args[0].Equals("cleanup", StringComparison.OrdinalIgnoreCase)
            ? CliMode.Cleanup
            : CliMode.Seed;

        var options = new CliOptions { Mode = mode };

        for (var i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--orders" when i + 1 < args.Length && int.TryParse(args[i + 1], out var parsedOrders):
                    options.Orders = Math.Max(0, parsedOrders);
                    i++;
                    break;
                case "--profiles" when i + 1 < args.Length && int.TryParse(args[i + 1], out var parsedProfiles):
                    options.Profiles = Math.Max(0, parsedProfiles);
                    i++;
                    break;
            }
        }

        return options;
    }

    public static void PrintHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  seed (default):   dotnet run -- seed --orders 250 --profiles 25");
        Console.WriteLine("  cleanup:          dotnet run -- cleanup");
    }
}
