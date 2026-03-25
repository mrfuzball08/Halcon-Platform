public interface IUsersService
{
	Task<List<UserResponse>> ListAsync(CancellationToken cancellationToken = default);
	Task<UserResponse> GetAsync(int id, CancellationToken cancellationToken = default);
	Task<UserResponse> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default);
	Task<UserResponse> UpdateAsync(int id, UserUpdateRequest request, CancellationToken cancellationToken = default);
	Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
