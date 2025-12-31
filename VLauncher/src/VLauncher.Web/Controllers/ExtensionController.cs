using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using VLauncher.Application.Users.Commands;

namespace VLauncher.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("ChromeExtension")]
public class ExtensionController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExtensionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterPendingUser([FromBody] RegisterPendingUserRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
            return BadRequest(new { error = "Email is required" });

        var result = await _mediator.Send(new CreatePendingUserCommand(request.Email));

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "User registered successfully", user = result.Data });
    }
}

public class RegisterPendingUserRequest
{
    public string Email { get; set; } = string.Empty;
}
