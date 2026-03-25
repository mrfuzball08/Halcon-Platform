public interface IAuthRepository
{
	Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
	Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
	Task<User?> GetByAuthUserIdAsync(string authUserId, CancellationToken cancellationToken = default);
	Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
	Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default);
}
