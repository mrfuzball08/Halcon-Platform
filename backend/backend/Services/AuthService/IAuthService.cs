public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task SeedAdminAsync(CancellationToken cancellationToken = default);
}
