public sealed class ApplicationOptions
{
    public required string CorsAllowedOrigin { get; init; }
    public required string SeedAdminUsername { get; init; }
    public required string SeedAdminEmail { get; init; }
    public required string SeedAdminPassword { get; init; }
    public required string SeedAdminRole { get; init; }
    public required string DefaultUserEmailDomain { get; init; }
}
