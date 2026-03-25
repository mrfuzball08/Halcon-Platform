public sealed class SupabaseAppOptions
{
    public required string Url { get; init; }
    public required string AnonKey { get; init; }
    public required string ServiceRoleKey { get; init; }
    public required string StorageBucket { get; init; }
    public required string Schema { get; init; }

    public string AuthUrl => $"{Url.TrimEnd('/')}/auth/v1";
}
