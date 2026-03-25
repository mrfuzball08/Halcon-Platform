public sealed class UsersService(
	IUsersRepository usersRepository,
	ISupabaseAuthGateway supabaseAuthGateway,
	ApplicationOptions applicationOptions) : IUsersService
{
	public async Task<List<UserResponse>> ListAsync(CancellationToken cancellationToken = default)
	{
		var users = await usersRepository.ListAsync(cancellationToken);
		return users.Select(x => x.ToResponse()).ToList();
	}

	public async Task<UserResponse> GetAsync(int id, CancellationToken cancellationToken = default)
	{
		var user = await usersRepository.GetByIdAsync(id, cancellationToken);
		if (user is null)
		{
			throw new ApiException("User not found.", StatusCodes.Status404NotFound);
		}

		return user.ToResponse();
	}

	public async Task<UserResponse> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
		{
			throw new ApiException("Username and password are required.", StatusCodes.Status400BadRequest);
		}

		var resolvedEmail = ResolveEmail(request.Username, request.Email);

		if (!DomainValidation.IsValidRole(request.Role))
		{
			throw new ApiException("Role is invalid.", StatusCodes.Status400BadRequest);
		}

		var exists = await usersRepository.GetByUsernameAsync(request.Username, cancellationToken);
		if (exists is not null)
		{
			throw new ApiException("Username is already taken.", StatusCodes.Status409Conflict);
		}

		var emailExists = await usersRepository.GetByEmailAsync(resolvedEmail, cancellationToken);
		if (emailExists is not null)
		{
			throw new ApiException("Email is already taken.", StatusCodes.Status409Conflict);
		}

		var authUser = await supabaseAuthGateway.CreateUserAsync(
			resolvedEmail,
			request.Password,
			new Dictionary<string, object>
			{
				["username"] = request.Username,
				["role"] = request.Role
			});

		if (string.IsNullOrWhiteSpace(authUser.Id))
		{
			throw new ApiException("Supabase auth did not return a valid user id.", StatusCodes.Status500InternalServerError);
		}

		var user = new User
		{
			AuthUserId = authUser.Id,
			Username = request.Username,
			Email = resolvedEmail,
			Role = request.Role
		};

		try
		{
			var created = await usersRepository.CreateAsync(user, cancellationToken);
			return created.ToResponse();
		}
		catch
		{
			await supabaseAuthGateway.DeleteUserAsync(authUser.Id);
			throw;
		}
	}

	public async Task<UserResponse> UpdateAsync(int id, UserUpdateRequest request, CancellationToken cancellationToken = default)
	{
		var user = await usersRepository.GetByIdAsync(id, cancellationToken);
		if (user is null)
		{
			throw new ApiException("User not found.", StatusCodes.Status404NotFound);
		}

		if (!string.IsNullOrWhiteSpace(request.Username) && !string.Equals(request.Username, user.Username, StringComparison.Ordinal))
		{
			var duplicate = await usersRepository.GetByUsernameAsync(request.Username, cancellationToken);
			if (duplicate is not null)
			{
				throw new ApiException("Username is already taken.", StatusCodes.Status409Conflict);
			}

			user.Username = request.Username;
		}

		if (!string.IsNullOrWhiteSpace(request.Email) && !string.Equals(request.Email, user.Email, StringComparison.OrdinalIgnoreCase))
		{
			var duplicateEmail = await usersRepository.GetByEmailAsync(request.Email, cancellationToken);
			if (duplicateEmail is not null)
			{
				throw new ApiException("Email is already taken.", StatusCodes.Status409Conflict);
			}

			user.Email = request.Email;
		}

		if (!string.IsNullOrWhiteSpace(request.Role))
		{
			if (!DomainValidation.IsValidRole(request.Role))
			{
				throw new ApiException("Role is invalid.", StatusCodes.Status400BadRequest);
			}

			user.Role = request.Role;
		}

		if (!string.IsNullOrWhiteSpace(request.Password))
		{
			await supabaseAuthGateway.UpdateUserByIdAsync(user.AuthUserId, null, request.Password);
		}

		if (!string.IsNullOrWhiteSpace(request.Email))
		{
			await supabaseAuthGateway.UpdateUserByIdAsync(user.AuthUserId, user.Email, null);
		}

		var updated = await usersRepository.UpdateAsync(user, cancellationToken);
		if (updated is null)
		{
			throw new ApiException("User update failed.", StatusCodes.Status500InternalServerError);
		}

		return updated.ToResponse();
	}

	public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
	{
		var user = await usersRepository.GetByIdAsync(id, cancellationToken);
		if (user is null)
		{
			throw new ApiException("User not found.", StatusCodes.Status404NotFound);
		}

		if (string.IsNullOrWhiteSpace(user.AuthUserId))
		{
			throw new ApiException("User profile has no auth mapping.", StatusCodes.Status500InternalServerError);
		}

		await supabaseAuthGateway.DeleteUserAsync(user.AuthUserId);

		var deleted = await usersRepository.DeleteAsync(id, cancellationToken);
		if (!deleted)
		{
			throw new ApiException("User not found.", StatusCodes.Status404NotFound);
		}
	}

	private string ResolveEmail(string username, string? requestedEmail)
	{
		if (!string.IsNullOrWhiteSpace(requestedEmail))
		{
			return requestedEmail.Trim();
		}

		var normalized = username.Trim().ToLowerInvariant().Replace(" ", ".");
		return $"{normalized}@{applicationOptions.DefaultUserEmailDomain}";
	}
}
