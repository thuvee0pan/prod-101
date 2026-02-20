using ExecutionOS.API.DTOs;
using ExecutionOS.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExecutionOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly GoalService _goalService;
    private readonly ProjectService _projectService;
    private readonly StreakService _streakService;
    private readonly WarningService _warningService;
    private readonly WeeklyReviewService _reviewService;
    private readonly DailyLogService _logService;

    public DashboardController(
        GoalService goalService,
        ProjectService projectService,
        StreakService streakService,
        WarningService warningService,
        WeeklyReviewService reviewService,
        DailyLogService logService)
    {
        _goalService = goalService;
        _projectService = projectService;
        _streakService = streakService;
        _warningService = warningService;
        _reviewService = reviewService;
        _logService = logService;
    }

    private Guid GetUserId() =>
        Guid.TryParse(Request.Headers["X-User-Id"].FirstOrDefault(), out var id) ? id : Guid.Empty;

    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get()
    {
        var userId = GetUserId();

        var goal = await _goalService.GetActiveGoal(userId);
        var projects = await _projectService.GetActiveProjects(userId);
        var streaks = await _streakService.GetStreaks(userId);
        var warnings = await _warningService.GetActiveWarnings(userId);
        var latestReview = await _reviewService.GetLatest(userId);
        var todayLog = await _logService.GetToday(userId);

        // Calculate this week's execution score
        var now = DateTime.UtcNow;
        var daysFromMonday = ((int)now.DayOfWeek + 6) % 7;
        var weekStart = DateOnly.FromDateTime(now.AddDays(-daysFromMonday));
        var weekEnd = DateOnly.FromDateTime(now);
        var weekLogs = await _logService.GetLogs(userId, weekStart, weekEnd);

        var score = new ExecutionScore(
            WeeklyDeepWorkMinutes: weekLogs.Sum(l => l.DeepWorkMinutes),
            WeeklyGymDays: weekLogs.Count(l => l.GymCompleted),
            WeeklyLearningMinutes: weekLogs.Sum(l => l.LearningMinutes),
            WeeklySoberDays: weekLogs.Count(l => l.AlcoholFree),
            OverallPercentage: CalculateOverallScore(weekLogs)
        );

        return Ok(new DashboardResponse(
            goal, projects, streaks, warnings, latestReview, todayLog, score
        ));
    }

    private static double CalculateOverallScore(List<DailyLogResponse> weekLogs)
    {
        if (weekLogs.Count == 0) return 0;

        var daysLogged = weekLogs.Count;
        var maxDays = Math.Min(7, daysLogged > 0 ? 7 : 1);

        // Each metric contributes 25% to overall score
        var deepWorkScore = weekLogs.Count(l => l.DeepWorkMinutes >= 120) / (double)maxDays;  // 2h+ target
        var gymScore = weekLogs.Count(l => l.GymCompleted) / (double)maxDays;
        var learningScore = weekLogs.Count(l => l.LearningMinutes >= 30) / (double)maxDays;   // 30min+ target
        var soberScore = weekLogs.Count(l => l.AlcoholFree) / (double)maxDays;

        return Math.Round((deepWorkScore + gymScore + learningScore + soberScore) / 4 * 100, 1);
    }
}
