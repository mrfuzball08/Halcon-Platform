public static class ConfigurationExtensions
{
    public static SupabaseAppOptions ToSupabaseOptions(this IConfiguration configuration)
    {
        return new SupabaseAppOptions
        {
            Url = configuration.Required("SUPABASE_URL"),
            AnonKey = configuration.Required("SUPABASE_ANON_KEY"),
            ServiceRoleKey = configuration.Required("SUPABASE_SERVICE_ROLE_KEY"),
            StorageBucket = configuration["SUPABASE_STORAGE_BUCKET"] ?? "order-evidence",
            Schema = configuration["SUPABASE_SCHEMA"] ?? "public"
        };
    }

    public static ApplicationOptions ToApplicationOptions(this IConfiguration configuration, bool isDevelopment)
    {
        return new ApplicationOptions
        {
            CorsAllowedOrigin = configuration["CORS_ALLOWED_ORIGIN"] ?? "http://localhost:3000",
            SeedAdminOnStartup = configuration.OptionalBool("SEED_ADMIN_ON_STARTUP", false),
            SeedAdminUsername = configuration.SeedValue("SEED_ADMIN_USERNAME", "admin", isDevelopment),
            SeedAdminEmail = configuration.SeedValue("SEED_ADMIN_EMAIL", "admin@halcon.local", isDevelopment),
            SeedAdminPassword = configuration.SeedValue("SEED_ADMIN_PASSWORD", "Admin123!", isDevelopment),
            SeedAdminRole = configuration.SeedValue("SEED_ADMIN_ROLE", "ADMIN", isDevelopment),
            SeedAdminToken = configuration["SEED_ADMIN_TOKEN"]
        };
    }

    private static string Required(this IConfiguration configuration, string key)
    {
        return configuration[key] ?? throw new InvalidOperationException($"Environment variable '{key}' is required.");
    }

    private static string SeedValue(this IConfiguration configuration, string key, string developmentDefault, bool isDevelopment)
    {
        var value = configuration[key];
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (isDevelopment)
        {
            return developmentDefault;
        }

        return string.Empty;
    }

    private static bool OptionalBool(this IConfiguration configuration, string key, bool defaultValue)
    {
        var raw = configuration[key];
        if (string.IsNullOrWhiteSpace(raw))
        {
            return defaultValue;
        }

        if (bool.TryParse(raw, out var parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException($"Environment variable '{key}' must be 'true' or 'false'.");
    }
}
