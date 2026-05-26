using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhilCharronWebFolio.Application.Accessibility.Commands;
using PhilCharronWebFolio.Application.Common.Messaging;

namespace PhilCharronWebFolio.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AccessibilityAuditsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost, Authorize]
    public async Task<IActionResult> Create([FromBody] CreateAccessibilityAuditCommand command, CancellationToken ct)
    {
        var id = await dispatcher.SendAsync<CreateAccessibilityAuditCommand, Guid>(command, ct);
        return Ok(new { Id = id });
    }
}
