using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("profiles")]
public class User : BaseModel
{
	[PrimaryKey("id", false)]
	public int Id { get; set; }

	[Column("auth_user_id")]
	public string AuthUserId { get; set; } = string.Empty;

	[Column("username")]
	public string Username { get; set; } = string.Empty;

	[Column("email")]
	public string Email { get; set; } = string.Empty;

	[Column("role")]
	public string Role { get; set; } = UserRole.SALES.ToString();
}
