using ExecutionOS.API.DTOs;
using ExecutionOS.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExecutionOS.API.Controllers;

[ApiController]
[Route("api/weekly-reviews")]
public class WeeklyReviewsController : ControllerBase
{
    private readonly WeeklyReviewService _reviewService;

    public WeeklyReviewsController(WeeklyReviewService reviewService) => _reviewService = reviewService;

    private Guid GetUserId() =>
        Guid.TryParse(Request.Headers["X-User-Id"].FirstOrDefault(), out var id) ? id : Guid.Empty;

    [HttpPost("generate")]
    public async Task<ActionResult<WeeklyReviewResponse>> Generate()
    {
        return Ok(await _reviewService.Generate(GetUserId()));
    }

    [HttpGet]
    public async Task<ActionResult<List<WeeklyReviewResponse>>> GetAll()
    {
        return Ok(await _reviewService.GetAll(GetUserId()));
    }

    [HttpGet("latest")]
    public async Task<ActionResult<WeeklyReviewResponse>> GetLatest()
    {
        var result = await _reviewService.GetLatest(GetUserId());
        return result == null ? NotFound(new { error = "No reviews generated yet." }) : Ok(result);
    }
}
