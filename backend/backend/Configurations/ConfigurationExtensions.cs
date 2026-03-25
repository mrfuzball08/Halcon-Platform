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

    public static ApplicationOptions ToApplicationOptions(this IConfiguration configuration)
    {
        return new ApplicationOptions
        {
            CorsAllowedOrigin = configuration["CORS_ALLOWED_ORIGIN"] ?? "http://localhost:3000",
            SeedAdminUsername = configuration["SEED_ADMIN_USERNAME"] ?? "admin",
            SeedAdminEmail = configuration["SEED_ADMIN_EMAIL"] ?? "admin@halcon.local",
            SeedAdminPassword = configuration["SEED_ADMIN_PASSWORD"] ?? "Admin123!",
            SeedAdminRole = configuration["SEED_ADMIN_ROLE"] ?? "ADMIN",
            DefaultUserEmailDomain = configuration["DEFAULT_USER_EMAIL_DOMAIN"] ?? "halcon.local"
        };
    }

    private static string Required(this IConfiguration configuration, string key)
    {
        return configuration[key] ?? throw new InvalidOperationException($"Environment variable '{key}' is required.");
    }
}
