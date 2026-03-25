public sealed class UserCreateRequest
{
	public string Username { get; set; } = string.Empty;
	public string? Email { get; set; }
	public string Password { get; set; } = string.Empty;
	public string Role { get; set; } = string.Empty;
}
