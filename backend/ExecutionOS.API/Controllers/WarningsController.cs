using ExecutionOS.API.DTOs;
using ExecutionOS.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExecutionOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarningsController : ControllerBase
{
    private readonly WarningService _warningService;

    public WarningsController(WarningService warningService) => _warningService = warningService;

    private Guid GetUserId() =>
        Guid.TryParse(Request.Headers["X-User-Id"].FirstOrDefault(), out var id) ? id : Guid.Empty;

    [HttpGet]
    public async Task<ActionResult<List<WarningResponse>>> GetActive()
    {
        return Ok(await _warningService.GetActiveWarnings(GetUserId()));
    }

    [HttpPut("{id}/acknowledge")]
    public async Task<ActionResult> Acknowledge(Guid id)
    {
        try
        {
            await _warningService.AcknowledgeWarning(GetUserId(), id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
