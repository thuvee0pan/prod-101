using ExecutionOS.API.DTOs;
using ExecutionOS.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExecutionOS.API.Controllers;

[ApiController]
[Route("api/daily-logs")]
public class DailyLogsController : ControllerBase
{
    private readonly DailyLogService _logService;
    private readonly StreakService _streakService;

    public DailyLogsController(DailyLogService logService, StreakService streakService)
    {
        _logService = logService;
        _streakService = streakService;
    }

    private Guid GetUserId() =>
        Guid.TryParse(Request.Headers["X-User-Id"].FirstOrDefault(), out var id) ? id : Guid.Empty;

    [HttpPost]
    public async Task<ActionResult<DailyLogResponse>> LogToday([FromBody] CreateDailyLogRequest request)
    {
        var result = await _logService.LogToday(GetUserId(), request);
        return Ok(result);
    }

    [HttpGet("today")]
    public async Task<ActionResult<DailyLogResponse>> GetToday()
    {
        var result = await _logService.GetToday(GetUserId());
        return result == null ? NotFound(new { error = "No log for today yet." }) : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<DailyLogResponse>>> GetLogs(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var start = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var end = to ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await _logService.GetLogs(GetUserId(), start, end));
    }

    [HttpGet("streaks")]
    public async Task<ActionResult<List<StreakResponse>>> GetStreaks()
    {
        return Ok(await _streakService.GetStreaks(GetUserId()));
    }
}
