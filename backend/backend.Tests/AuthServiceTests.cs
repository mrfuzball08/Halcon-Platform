using Xunit;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task SeedAdminAsync_WhenAdminAlreadyExistsByUsername_DoesNothing()
    {
        var repository = new FakeAuthRepository();
        repository.UsernameResponses.Enqueue(new User { Id = 1, Username = "admin" });

        var gateway = new FakeSupabaseAuthGateway();
        var service = new AuthService(repository, gateway, BuildOptions());

        await service.SeedAdminAsync();

        Assert.Equal(0, gateway.CreateUserCalls);
        Assert.Equal(0, repository.CreateCalls);
    }

    [Fact]
    public async Task SeedAdminAsync_WhenAuthUserAlreadyExistsFromConcurrentSeed_Completes()
    {
        var repository = new FakeAuthRepository();
        repository.UsernameResponses.Enqueue(null);
        repository.UsernameResponses.Enqueue(new User { Id = 1, Username = "admin" });

        var gateway = new FakeSupabaseAuthGateway
        {
            CreateUserException = new Exception("User already registered")
        };

        var service = new AuthService(repository, gateway, BuildOptions());

        await service.SeedAdminAsync();

        Assert.Equal(1, gateway.CreateUserCalls);
        Assert.Equal(0, repository.CreateCalls);
    }

    [Fact]
    public async Task SeedAdminAsync_WhenSeedConfigIsMissing_ThrowsApiException()
    {
        var repository = new FakeAuthRepository();
        var gateway = new FakeSupabaseAuthGateway();

        var options = new ApplicationOptions
        {
            CorsAllowedOrigin = "http://localhost:3000",
            SeedAdminOnStartup = true,
            SeedAdminUsername = "admin",
            SeedAdminEmail = string.Empty,
            SeedAdminPassword = "Admin123!",
            SeedAdminRole = nameof(UserRole.ADMIN),
            SeedAdminToken = "seed-token"
        };

        var service = new AuthService(repository, gateway, options);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.SeedAdminAsync());

        Assert.Equal(500, exception.StatusCode);
    }

    private static ApplicationOptions BuildOptions()
    {
        return new ApplicationOptions
        {
            CorsAllowedOrigin = "http://localhost:3000",
            SeedAdminOnStartup = true,
            SeedAdminUsername = "admin",
            SeedAdminEmail = "admin@halcon.local",
            SeedAdminPassword = "Admin123!",
            SeedAdminRole = nameof(UserRole.ADMIN),
            SeedAdminToken = "seed-token"
        };
    }

    private sealed class FakeAuthRepository : IAuthRepository
    {
        public Queue<User?> UsernameResponses { get; } = new();
        public Queue<User?> EmailResponses { get; } = new();
        public int CreateCalls { get; private set; }

        public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (UsernameResponses.Count > 0)
            {
                return Task.FromResult(UsernameResponses.Dequeue());
            }

            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (EmailResponses.Count > 0)
            {
                return Task.FromResult(EmailResponses.Dequeue());
            }

            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByAuthUserIdAsync(string authUserId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            CreateCalls++;
            return Task.FromResult(user);
        }

        public Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeSupabaseAuthGateway : ISupabaseAuthGateway
    {
        public Exception? CreateUserException { get; set; }
        public int CreateUserCalls { get; private set; }

        public Task<Supabase.Gotrue.Session> SignInWithPasswordAsync(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<Supabase.Gotrue.User> CreateUserAsync(string email, string password, IDictionary<string, object>? userMetadata = null)
        {
            CreateUserCalls++;
            if (CreateUserException is not null)
            {
                throw CreateUserException;
            }

            throw new InvalidOperationException("Unexpected CreateUserAsync call in this test.");
        }

        public Task<Supabase.Gotrue.User> UpdateUserByIdAsync(string authUserId, string? email = null, string? password = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUserAsync(string authUserId)
        {
            return Task.CompletedTask;
        }
    }
}
