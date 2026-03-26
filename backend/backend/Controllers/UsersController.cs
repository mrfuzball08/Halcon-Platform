using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
[Authorize(Roles = nameof(UserRole.ADMIN))]
public sealed class UsersController(IUsersService usersService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> List(CancellationToken cancellationToken)
    {
        return Ok(await usersService.ListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        return Ok(await usersService.GetAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] UserCreateRequest request, CancellationToken cancellationToken)
    {
        var created = await usersService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponse>> Update([FromRoute] int id, [FromBody] UserUpdateRequest request, CancellationToken cancellationToken)
    {
        return Ok(await usersService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        await usersService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
