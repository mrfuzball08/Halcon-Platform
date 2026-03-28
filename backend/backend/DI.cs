using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

public static class DependencyInjection
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var supabaseOptions = configuration.ToSupabaseOptions();
        var appOptions = configuration.ToApplicationOptions(environment.IsDevelopment());

        services.AddSingleton(supabaseOptions);
        services.AddSingleton(appOptions);

        services.AddControllers();
        services.AddOpenApi();

        services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                policy.WithOrigins(appOptions.CorsAllowedOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = supabaseOptions.AuthUrl;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = supabaseOptions.AuthUrl,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtAuth");
                        logger.LogError(context.Exception, "JWT authentication failed: {Message}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var authUserId = context.Principal?.FindFirstValue("sub");
                        if (string.IsNullOrWhiteSpace(authUserId))
                        {
                            context.Fail("Token does not include a valid sub claim.");
                            return;
                        }

                        var usersRepository = context.HttpContext.RequestServices.GetRequiredService<IUsersRepository>();
                        var user = await usersRepository.GetByAuthUserIdAsync(authUserId, context.HttpContext.RequestAborted);

                        if (user is null)
                        {
                            context.Fail("User profile not found.");
                            return;
                        }

                        if (context.Principal?.Identity is ClaimsIdentity identity)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
                            identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
                        }
                    }
                };
            });

        services.AddAuthorization();

        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<SupabaseAppOptions>();
            var client = new Supabase.Client(
                settings.Url,
                settings.ServiceRoleKey,
                new Supabase.SupabaseOptions
                {
                    Schema = settings.Schema,
                    AutoConnectRealtime = false,
                    AutoRefreshToken = false
                });

            client.InitializeAsync().GetAwaiter().GetResult();
            return client;
        });

        services.AddSingleton<AppDbContext>();

        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        services.AddScoped<IPublicRepository, PublicRepository>();
        services.AddScoped<IImagesRepository, ImagesRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISupabaseAuthGateway, SupabaseAuthGateway>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IOrdersService, OrdersService>();
        services.AddScoped<IPublicService, PublicService>();
        services.AddScoped<IImagesService, ImagesService>();
    }
}