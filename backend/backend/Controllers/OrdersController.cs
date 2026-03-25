using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController(IOrdersService ordersService, IImagesService imagesService) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<List<OrderResponse>>> List(
		[FromQuery] string? invoice,
		[FromQuery] string? customer,
		[FromQuery] string? date,
		[FromQuery] string? status,
		CancellationToken cancellationToken)
	{
		return Ok(await ordersService.ListAsync(invoice, customer, date, status, includeDeleted: false, cancellationToken));
	}

	[HttpGet("deleted")]
	[Authorize(Roles = nameof(UserRole.ADMIN))]
	public async Task<ActionResult<List<OrderResponse>>> ListDeleted(CancellationToken cancellationToken)
	{
		return Ok(await ordersService.ListAsync(null, null, null, null, includeDeleted: true, cancellationToken));
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<OrderResponse>> Get([FromRoute] int id, CancellationToken cancellationToken)
	{
		return Ok(await ordersService.GetAsync(id, cancellationToken));
	}

	[HttpPost]
	[Authorize(Roles = nameof(UserRole.SALES) + "," + nameof(UserRole.ADMIN))]
	public async Task<ActionResult<OrderResponse>> Create([FromBody] OrderCreateRequest request, CancellationToken cancellationToken)
	{
		var created = await ordersService.CreateAsync(request, cancellationToken);
		return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
	}

	[HttpPut("{id:int}")]
	[Authorize(Roles = nameof(UserRole.SALES) + "," + nameof(UserRole.ADMIN))]
	public async Task<ActionResult<OrderResponse>> Update([FromRoute] int id, [FromBody] OrderUpdateRequest request, CancellationToken cancellationToken)
	{
		return Ok(await ordersService.UpdateAsync(id, request, cancellationToken));
	}

	[HttpPatch("{id:int}/status")]
	[Authorize(Roles = nameof(UserRole.ADMIN) + "," + nameof(UserRole.WAREHOUSE) + "," + nameof(UserRole.ROUTE))]
	public async Task<ActionResult<OrderResponse>> UpdateStatus([FromRoute] int id, [FromBody] OrderStatusUpdateRequest request, CancellationToken cancellationToken)
	{
		var actorRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
		return Ok(await ordersService.UpdateStatusAsync(id, request.Status, actorRole, cancellationToken));
	}

	[HttpDelete("{id:int}")]
	[Authorize(Roles = nameof(UserRole.ADMIN) + "," + nameof(UserRole.SALES))]
	public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
	{
		await ordersService.DeleteAsync(id, cancellationToken);
		return NoContent();
	}

	[HttpPatch("{id:int}/restore")]
	[Authorize(Roles = nameof(UserRole.ADMIN))]
	public async Task<ActionResult<OrderResponse>> Restore([FromRoute] int id, CancellationToken cancellationToken)
	{
		return Ok(await ordersService.RestoreAsync(id, cancellationToken));
	}

	[HttpPost("{id:int}/photos/loading")]
	[Authorize(Roles = nameof(UserRole.ROUTE) + "," + nameof(UserRole.ADMIN))]
	public async Task<ActionResult<OrderResponse>> UploadLoadingPhoto([FromRoute] int id, [FromForm] IFormFile file, CancellationToken cancellationToken)
	{
		return Ok(await imagesService.UploadLoadingPhotoAsync(id, file, cancellationToken));
	}

	[HttpPost("{id:int}/photos/delivery")]
	[Authorize(Roles = nameof(UserRole.ROUTE) + "," + nameof(UserRole.ADMIN))]
	public async Task<ActionResult<OrderResponse>> UploadDeliveryPhoto([FromRoute] int id, [FromForm] IFormFile file, CancellationToken cancellationToken)
	{
		return Ok(await imagesService.UploadDeliveryPhotoAsync(id, file, cancellationToken));
	}
}
