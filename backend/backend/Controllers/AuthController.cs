using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthService authService,
    IWebHostEnvironment environment,
    ApplicationOptions applicationOptions) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        return Ok(await authService.LoginAsync(request, cancellationToken));
    }

    [HttpPost("seed")]
    [AllowAnonymous]
    public async Task<IActionResult> Seed(
        [FromHeader(Name = "X-Seed-Token")] string? seedToken,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            throw new ApiException("Not found.", StatusCodes.Status404NotFound);
        }

        if (string.IsNullOrWhiteSpace(applicationOptions.SeedAdminToken))
        {
            throw new ApiException("Seed endpoint is disabled.", StatusCodes.Status403Forbidden);
        }

        if (!IsValidSeedToken(seedToken, applicationOptions.SeedAdminToken))
        {
            throw new ApiException("Invalid seed token.", StatusCodes.Status401Unauthorized);
        }

        await authService.SeedAdminAsync(cancellationToken);
        return NoContent();
    }

    private static bool IsValidSeedToken(string? seedToken, string expectedToken)
    {
        if (string.IsNullOrWhiteSpace(seedToken))
        {
            return false;
        }

        var providedBytes = Encoding.UTF8.GetBytes(seedToken);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedToken);

        return CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }
}
