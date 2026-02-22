using ExecutionOS.API.Data;
using ExecutionOS.API.DTOs;
using ExecutionOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExecutionOS.API.Services;

public class GoalService
{
    private readonly AppDbContext _db;
    private readonly ILogger<GoalService> _logger;

    public GoalService(AppDbContext db, ILogger<GoalService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<GoalResponse> CreateGoal(Guid userId, CreateGoalRequest request)
    {
        var activeGoal = await _db.Goals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.Status == GoalStatus.Active);

        if (activeGoal != null)
            throw new InvalidOperationException(
                "You already have an active 90-day goal. Complete or abandon it before starting a new one.");

        var goal = new Goal
        {
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(90)
        };

        _db.Goals.Add(goal);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Goal created — GoalId: {GoalId}, User: {UserId}",
            goal.Id.ToString()[..8], userId.ToString()[..8]);

        return MapToResponse(goal);
    }

    public async Task<GoalResponse?> GetActiveGoal(Guid userId)
    {
        var goal = await _db.Goals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.Status == GoalStatus.Active);

        return goal == null ? null : MapToResponse(goal);
    }

    public async Task<List<GoalResponse>> GetAllGoals(Guid userId)
    {
        var goals = await _db.Goals
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return goals.Select(MapToResponse).ToList();
    }

    public async Task<GoalResponse> CompleteGoal(Guid userId, Guid goalId)
    {
        var goal = await _db.Goals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId && g.Status == GoalStatus.Active)
            ?? throw new InvalidOperationException("Active goal not found.");

        goal.Status = GoalStatus.Completed;
        goal.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Goal completed — GoalId: {GoalId}, User: {UserId}",
            goalId.ToString()[..8], userId.ToString()[..8]);

        return MapToResponse(goal);
    }

    public async Task<GoalResponse> AbandonGoal(Guid userId, Guid goalId, AbandonGoalRequest request)
    {
        var goal = await _db.Goals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId && g.Status == GoalStatus.Active)
            ?? throw new InvalidOperationException("Active goal not found.");

        goal.Status = GoalStatus.Abandoned;
        goal.AbandonReason = request.Reason;
        goal.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Goal abandoned — GoalId: {GoalId}, User: {UserId}",
            goalId.ToString()[..8], userId.ToString()[..8]);

        return MapToResponse(goal);
    }

    private static GoalResponse MapToResponse(Goal goal)
    {
        var now = DateTime.UtcNow;
        var daysRemaining = Math.Max(0, (int)(goal.EndDate - now).TotalDays);
        var daysElapsed = (int)(now - goal.StartDate).TotalDays;

        return new GoalResponse(
            goal.Id,
            goal.Title,
            goal.Description,
            goal.StartDate,
            goal.EndDate,
            goal.Status.ToString(),
            daysRemaining,
            daysElapsed,
            goal.CreatedAt
        );
    }
}
