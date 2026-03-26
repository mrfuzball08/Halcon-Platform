using Xunit;

public sealed class UsersServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenEmailIsMissing_ThrowsBadRequest()
    {
        var repository = new FakeUsersRepository();
        var authGateway = new FakeSupabaseAuthGateway();
        var service = new UsersService(repository, authGateway);

        var request = new UserCreateRequest
        {
            Username = "new.user",
            Email = string.Empty,
            Password = "Password123!",
            Role = nameof(UserRole.SALES)
        };

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(request));

        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_WhenAuthDeleteCascadesProfile_DoesNotThrow()
    {
        var repository = new FakeUsersRepository
        {
            InitialGetByIdResult = new User
            {
                Id = 10,
                AuthUserId = "auth-10",
                Username = "u10",
                Email = "u10@example.com",
                Role = nameof(UserRole.ADMIN)
            },
            DeleteResult = false,
            AfterDeleteGetByIdResult = null
        };

        var authGateway = new FakeSupabaseAuthGateway();
        var service = new UsersService(repository, authGateway);

        await service.DeleteAsync(10);

        Assert.True(repository.DeleteCalled);
        Assert.Equal("auth-10", authGateway.DeletedAuthUserId);
    }

    [Fact]
    public async Task DeleteAsync_WhenProfileStillExistsAfterDeleteFailure_ThrowsServerError()
    {
        var persistedUser = new User
        {
            Id = 11,
            AuthUserId = "auth-11",
            Username = "u11",
            Email = "u11@example.com",
            Role = nameof(UserRole.ADMIN)
        };

        var repository = new FakeUsersRepository
        {
            InitialGetByIdResult = persistedUser,
            DeleteResult = false,
            AfterDeleteGetByIdResult = persistedUser
        };

        var authGateway = new FakeSupabaseAuthGateway();
        var service = new UsersService(repository, authGateway);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.DeleteAsync(11));

        Assert.Equal(500, exception.StatusCode);
        Assert.Equal("auth-11", authGateway.DeletedAuthUserId);
    }

    private sealed class FakeUsersRepository : IUsersRepository
    {
        private int getByIdCalls;

        public User? InitialGetByIdResult { get; set; }
        public User? AfterDeleteGetByIdResult { get; set; }
        public bool DeleteResult { get; set; }
        public bool DeleteCalled { get; private set; }

        public Task<List<User>> ListAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<User>());
        }

        public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            getByIdCalls++;
            if (getByIdCalls == 1)
            {
                return Task.FromResult(InitialGetByIdResult);
            }

            return Task.FromResult(AfterDeleteGetByIdResult);
        }

        public Task<User?> GetByAuthUserIdAsync(string authUserId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            DeleteCalled = true;
            return Task.FromResult(DeleteResult);
        }
    }

    private sealed class FakeSupabaseAuthGateway : ISupabaseAuthGateway
    {
        public string? DeletedAuthUserId { get; private set; }

        public Task<Supabase.Gotrue.Session> SignInWithPasswordAsync(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<Supabase.Gotrue.User> CreateUserAsync(string email, string password, IDictionary<string, object>? userMetadata = null)
        {
            throw new NotImplementedException();
        }

        public Task<Supabase.Gotrue.User> UpdateUserByIdAsync(string authUserId, string? email = null, string? password = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUserAsync(string authUserId)
        {
            DeletedAuthUserId = authUserId;
            return Task.CompletedTask;
        }
    }
}
