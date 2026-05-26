using Microsoft.AspNetCore.Mvc;
using PhilCharronWebFolio.Application.Contact.Commands;
using PhilCharronWebFolio.Application.Common.Messaging;
using Microsoft.AspNetCore.Authorization;

namespace PhilCharronWebFolio.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ContactController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost("send"), AllowAnonymous]
    public async Task<IActionResult> Send([FromBody] SendContactMessageCommand command, CancellationToken ct)
    {
        var result = await dispatcher.SendAsync<SendContactMessageCommand, bool>(command, ct);
        return Ok(result);
    }
}
