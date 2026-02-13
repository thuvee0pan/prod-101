using ExecutionOS.API.DTOs;
using ExecutionOS.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExecutionOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoalsController : ControllerBase
{
    private readonly GoalService _goalService;

    public GoalsController(GoalService goalService) => _goalService = goalService;

    // TODO: Replace with actual auth user ID extraction
    private Guid GetUserId() => Guid.Parse(Request.Headers["X-User-Id"].FirstOrDefault() ?? Guid.Empty.ToString());

    [HttpPost]
    public async Task<ActionResult<GoalResponse>> Create([FromBody] CreateGoalRequest request)
    {
        try
        {
            var result = await _goalService.CreateGoal(GetUserId(), request);
            return CreatedAtAction(nameof(GetActive), result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<GoalResponse>> GetActive()
    {
        var result = await _goalService.GetActiveGoal(GetUserId());
        return result == null ? NotFound(new { error = "No active goal. Set one." }) : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<GoalResponse>>> GetAll()
    {
        return Ok(await _goalService.GetAllGoals(GetUserId()));
    }

    [HttpPut("{id}/complete")]
    public async Task<ActionResult<GoalResponse>> Complete(Guid id)
    {
        try
        {
            return Ok(await _goalService.CompleteGoal(GetUserId(), id));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/abandon")]
    public async Task<ActionResult<GoalResponse>> Abandon(Guid id, [FromBody] AbandonGoalRequest request)
    {
        try
        {
            return Ok(await _goalService.AbandonGoal(GetUserId(), id, request));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
