public interface ISupabaseAuthGateway
{
    Task<Supabase.Gotrue.Session> SignInWithPasswordAsync(string email, string password);
    Task<Supabase.Gotrue.User> CreateUserAsync(string email, string password, IDictionary<string, object>? userMetadata = null);
    Task<Supabase.Gotrue.User> UpdateUserByIdAsync(string authUserId, string? email = null, string? password = null);
    Task DeleteUserAsync(string authUserId);
}
