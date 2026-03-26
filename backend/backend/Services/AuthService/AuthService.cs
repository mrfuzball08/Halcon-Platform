public sealed class AuthService(
    IAuthRepository authRepository,
    ISupabaseAuthGateway supabaseAuthGateway,
    ApplicationOptions applicationOptions) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ApiException("Username and password are required.", StatusCodes.Status400BadRequest);
        }

        var user = await authRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null)
        {
            throw new ApiException("Invalid credentials.", StatusCodes.Status401Unauthorized);
        }

        try
        {
            var session = await supabaseAuthGateway.SignInWithPasswordAsync(user.Email, request.Password);
            if (string.IsNullOrWhiteSpace(session.AccessToken))
            {
                throw new ApiException("Supabase did not return an access token.", StatusCodes.Status401Unauthorized);
            }

            return new LoginResponse
            {
                Token = session.AccessToken
            };
        }
        catch
        {
            throw new ApiException("Invalid credentials.", StatusCodes.Status401Unauthorized);
        }
    }

    public async Task SeedAdminAsync(CancellationToken cancellationToken = default)
    {
        var existing = await authRepository.GetByUsernameAsync(applicationOptions.SeedAdminUsername, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        if (!DomainValidation.IsValidRole(applicationOptions.SeedAdminRole))
        {
            throw new ApiException("SEED_ADMIN_ROLE must be a valid role.", StatusCodes.Status500InternalServerError);
        }

        var authUser = await supabaseAuthGateway.CreateUserAsync(
            applicationOptions.SeedAdminEmail,
            applicationOptions.SeedAdminPassword,
            new Dictionary<string, object>
            {
                ["username"] = applicationOptions.SeedAdminUsername,
                ["role"] = applicationOptions.SeedAdminRole
            });

        if (string.IsNullOrWhiteSpace(authUser.Id))
        {
            throw new ApiException("Supabase auth did not return a valid user id.", StatusCodes.Status500InternalServerError);
        }

        var admin = new User
        {
            AuthUserId = authUser.Id,
            Username = applicationOptions.SeedAdminUsername,
            Email = applicationOptions.SeedAdminEmail,
            Role = applicationOptions.SeedAdminRole
        };

        await authRepository.CreateAsync(admin, cancellationToken);
    }
}
