using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/public")]
public sealed class PublicController(IPublicService publicService) : ControllerBase
{
    [HttpGet("orders/track")]
    [AllowAnonymous]
    public async Task<ActionResult<PublicOrderTrackResponse>> Track(
        [FromQuery] string invoice,
        [FromQuery] string customer,
        CancellationToken cancellationToken)
    {
        return Ok(await publicService.TrackAsync(invoice, customer, cancellationToken));
    }
}
