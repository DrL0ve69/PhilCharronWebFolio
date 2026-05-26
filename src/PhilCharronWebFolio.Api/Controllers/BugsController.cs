using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhilCharronWebFolio.Application.Bugs.Commands;
using PhilCharronWebFolio.Application.Common.Messaging;

namespace PhilCharronWebFolio.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BugsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost, Authorize]
    public async Task<IActionResult> Create([FromBody] CreateBugReportCommand command, CancellationToken ct)
    {
        var bugId = await dispatcher.SendAsync<CreateBugReportCommand, Guid>(command, ct);
        return Ok(new { Id = bugId }); // Ou CreatedAtAction(...)
    }
}
