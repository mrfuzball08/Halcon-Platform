var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

var app = builder.Build();

var appOptions = app.Services.GetRequiredService<ApplicationOptions>();
if (appOptions.SeedAdminOnStartup)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupSeed");
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

    try
    {
        await authService.SeedAdminAsync(app.Lifetime.ApplicationStopping);
        logger.LogInformation("Startup admin seeding completed.");
    }
    catch (Exception exception)
    {
        logger.LogCritical(exception, "Startup admin seeding failed.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
