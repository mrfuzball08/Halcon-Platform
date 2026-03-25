namespace Backend.Migrations;

public sealed class ProfileEntity
{
    public int Id { get; set; }
    public Guid AuthUserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
