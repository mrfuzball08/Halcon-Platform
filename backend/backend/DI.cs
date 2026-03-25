public static class DependencyInjection
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
    }
}