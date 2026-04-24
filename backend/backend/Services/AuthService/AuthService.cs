public sealed class AuthService(
    IAuthRepository authRepository,
    ISupabaseAuthGateway supabaseAuthGateway,
    ApplicationOptions applicationOptions) : IAuthService
{
    private const int SeedConsistencyAttempts = 3;
    private static readonly TimeSpan SeedConsistencyRetryDelay = TimeSpan.FromMilliseconds(100);

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
                Token = session.AccessToken,
                Role = user.Role
            };
        }
        catch
        {
            throw new ApiException($"[SignInWithPasswordAsync] Invalid credentials.", StatusCodes.Status401Unauthorized);
        }
    }

    public async Task SeedAdminAsync(CancellationToken cancellationToken = default)
    {
        EnsureSeedConfiguration();

        if (await HasSeededAdminAsync(cancellationToken))
        {
            return;
        }

        if (!DomainValidation.IsValidRole(applicationOptions.SeedAdminRole))
        {
            throw new ApiException("SEED_ADMIN_ROLE must be a valid role.", StatusCodes.Status500InternalServerError);
        }

        Supabase.Gotrue.User authUser;
        try
        {
            authUser = await supabaseAuthGateway.CreateUserAsync(
                applicationOptions.SeedAdminEmail,
                applicationOptions.SeedAdminPassword,
                new Dictionary<string, object>
                {
                    ["username"] = applicationOptions.SeedAdminUsername,
                    ["role"] = applicationOptions.SeedAdminRole
                });
        }
        catch (Exception exception) when (IsLikelyDuplicateException(exception))
        {
            if (await WaitForSeededAdminAsync(cancellationToken))
            {
                return;
            }

            throw new ApiException("Admin seed identity already exists but profile is not available.", StatusCodes.Status500InternalServerError);
        }

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

        try
        {
            await authRepository.CreateAsync(admin, cancellationToken);
        }
        catch (Exception exception) when (IsLikelyDuplicateException(exception))
        {
            if (await WaitForSeededAdminAsync(cancellationToken))
            {
                return;
            }

            await TryDeleteAuthUserAsync(authUser.Id);
            throw;
        }
        catch
        {
            await TryDeleteAuthUserAsync(authUser.Id);

            throw;
        }
    }

    private void EnsureSeedConfiguration()
    {
        if (string.IsNullOrWhiteSpace(applicationOptions.SeedAdminUsername)
            || string.IsNullOrWhiteSpace(applicationOptions.SeedAdminEmail)
            || string.IsNullOrWhiteSpace(applicationOptions.SeedAdminPassword)
            || string.IsNullOrWhiteSpace(applicationOptions.SeedAdminRole))
        {
            throw new ApiException("Seed admin configuration is incomplete.", StatusCodes.Status500InternalServerError);
        }
    }

    private async Task<bool> HasSeededAdminAsync(CancellationToken cancellationToken)
    {
        var byUsername = await authRepository.GetByUsernameAsync(applicationOptions.SeedAdminUsername, cancellationToken);
        if (byUsername is not null)
        {
            return true;
        }

        var byEmail = await authRepository.GetByEmailAsync(applicationOptions.SeedAdminEmail, cancellationToken);
        return byEmail is not null;
    }

    private async Task<bool> WaitForSeededAdminAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < SeedConsistencyAttempts; attempt++)
        {
            if (await HasSeededAdminAsync(cancellationToken))
            {
                return true;
            }

            if (attempt < SeedConsistencyAttempts - 1)
            {
                await Task.Delay(SeedConsistencyRetryDelay, cancellationToken);
            }
        }

        return false;
    }

    private static bool IsLikelyDuplicateException(Exception exception)
    {
        var details = exception.ToString();
        return details.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || details.Contains("already exists", StringComparison.OrdinalIgnoreCase)
            || details.Contains("already registered", StringComparison.OrdinalIgnoreCase)
            || details.Contains("23505", StringComparison.OrdinalIgnoreCase);
    }

    private async Task TryDeleteAuthUserAsync(string authUserId)
    {
        try
        {
            await supabaseAuthGateway.DeleteUserAsync(authUserId);
        }
        catch
        {
            // Keep the original failure when rollback cleanup also fails.
        }
    }
}
