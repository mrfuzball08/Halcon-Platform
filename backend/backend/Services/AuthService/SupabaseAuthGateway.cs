public sealed class SupabaseAuthGateway(SupabaseAppOptions supabaseOptions) : ISupabaseAuthGateway
{
    private readonly Supabase.Gotrue.StatelessClient _client = new();

    private readonly Supabase.Gotrue.StatelessClient.StatelessClientOptions _options = new()
    {
        Url = $"{supabaseOptions.Url.TrimEnd('/')}/auth/v1",
        AllowUnconfirmedUserSessions = true
    };

    public async Task<Supabase.Gotrue.Session> SignInWithPasswordAsync(string email, string password)
    {
        var session = await _client.SignIn(email, password, _options);
        if (session is null)
        {
            throw new ApiException("Supabase login failed.", StatusCodes.Status401Unauthorized);
        }

        return session;
    }

    public async Task<Supabase.Gotrue.User> CreateUserAsync(string email, string password, IDictionary<string, object>? userMetadata = null)
    {
        var user = await _client.CreateUser(
            supabaseOptions.ServiceRoleKey,
            _options,
            email,
            password,
            new Supabase.Gotrue.AdminUserAttributes
            {
                Email = email,
                Password = password,
                EmailConfirm = true,
                UserMetadata = userMetadata is null ? new Dictionary<string, object>() : new Dictionary<string, object>(userMetadata)
            });

        if (user is null)
        {
            throw new ApiException("Supabase user creation failed.", StatusCodes.Status500InternalServerError);
        }

        return user;
    }

    public async Task<Supabase.Gotrue.User> UpdateUserByIdAsync(string authUserId, string? email = null, string? password = null)
    {
        var user = await _client.UpdateUserById(
            supabaseOptions.ServiceRoleKey,
            _options,
            authUserId,
            new Supabase.Gotrue.AdminUserAttributes
            {
                Email = email,
                Password = password,
                EmailConfirm = true
            });

        if (user is null)
        {
            throw new ApiException("Supabase user update failed.", StatusCodes.Status500InternalServerError);
        }

        return user;
    }

    public async Task DeleteUserAsync(string authUserId)
    {
        await _client.DeleteUser(authUserId, supabaseOptions.ServiceRoleKey, _options);
    }
}
