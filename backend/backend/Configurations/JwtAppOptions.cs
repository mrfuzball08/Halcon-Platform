public sealed class JwtAppOptions
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string Secret { get; init; }
    public required int ExpMinutes { get; init; }
}
