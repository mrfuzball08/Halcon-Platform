using Supabase.Postgrest;

public sealed class UsersRepository(AppDbContext dbContext) : IUsersRepository
{
	public async Task<List<User>> ListAsync(CancellationToken cancellationToken = default)
	{
		var response = await dbContext.Client
			.From<User>()
			.Order("id", Constants.Ordering.Ascending)
			.Get(cancellationToken);

		return response.Models;
	}

	public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
	{
		var response = await dbContext.Client
			.From<User>()
			.Filter("id", Constants.Operator.Equals, id)
			.Limit(1)
			.Get(cancellationToken);

		return response.Models.FirstOrDefault();
	}

	public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
	{
		var response = await dbContext.Client
			.From<User>()
			.Filter("username", Constants.Operator.Equals, username)
			.Limit(1)
			.Get(cancellationToken);

		return response.Models.FirstOrDefault();
	}

	public async Task<User?> GetByAuthUserIdAsync(string authUserId, CancellationToken cancellationToken = default)
	{
		var response = await dbContext.Client
			.From<User>()
			.Filter("auth_user_id", Constants.Operator.Equals, authUserId)
			.Limit(1)
			.Get(cancellationToken);

		return response.Models.FirstOrDefault();
	}

	public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
	{
		var response = await dbContext.Client
			.From<User>()
			.Filter("email", Constants.Operator.Equals, email)
			.Limit(1)
			.Get(cancellationToken);

		return response.Models.FirstOrDefault();
	}

	public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
	{
		var response = await dbContext.Client
			.From<User>()
			.Insert(user, cancellationToken: cancellationToken);

		return response.Models.First();
	}

	public async Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default)
	{
		var response = await dbContext.Client
			.From<User>()
			.Filter("id", Constants.Operator.Equals, user.Id)
			.Update(user, cancellationToken: cancellationToken);

		return response.Models.FirstOrDefault();
	}

	public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
	{
		var existing = await GetByIdAsync(id, cancellationToken);
		if (existing is null)
		{
			return false;
		}

		await dbContext.Client
			.From<User>()
			.Filter("id", Constants.Operator.Equals, id)
			.Delete(cancellationToken: cancellationToken);

		return true;
	}
}
