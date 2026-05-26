using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhilCharronWebFolio.Application.Auth.Commands;
using PhilCharronWebFolio.Application.Auth.DTOs;
using PhilCharronWebFolio.Application.Auth.Queries;
using PhilCharronWebFolio.Application.Common.Messaging;

namespace PhilCharronWebFolio.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login(LoginCommand command) =>
        Ok(await dispatcher.SendAsync<LoginCommand, AuthResponseDto>(command));
    

    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct) =>
        Ok(await dispatcher.SendAsync<RegisterCommand, AuthResponseDto>(command, ct));

    [HttpGet("profile"), Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        return Ok(await dispatcher.SendAsync<GetProfileQuery, ProfileDto>(new GetProfileQuery(Guid.Parse(userId)), default));
    }
}
