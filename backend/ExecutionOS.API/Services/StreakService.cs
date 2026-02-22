using ExecutionOS.API.Data;
using ExecutionOS.API.DTOs;
using ExecutionOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExecutionOS.API.Services;

public class StreakService
{
    private readonly AppDbContext _db;
    private readonly ILogger<StreakService> _logger;

    public StreakService(AppDbContext db, ILogger<StreakService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task UpdateStreaks(Guid userId, DailyLog log)
    {
        await UpdateStreak(userId, StreakType.DeepWork, log.DeepWorkMinutes > 0, log.LogDate);
        await UpdateStreak(userId, StreakType.Gym, log.GymCompleted, log.LogDate);
        await UpdateStreak(userId, StreakType.Learning, log.LearningMinutes > 0, log.LogDate);
        await UpdateStreak(userId, StreakType.Sober, log.AlcoholFree, log.LogDate);
    }

    public async Task<List<StreakResponse>> GetStreaks(Guid userId)
    {
        var streaks = await _db.Streaks
            .Where(s => s.UserId == userId)
            .ToListAsync();

        // Initialize missing streaks
        var allTypes = Enum.GetValues<StreakType>();
        var existingTypes = streaks.Select(s => s.StreakType).ToHashSet();

        foreach (var type in allTypes.Where(t => !existingTypes.Contains(t)))
        {
            streaks.Add(new Streak
            {
                UserId = userId,
                StreakType = type,
                CurrentCount = 0,
                LongestCount = 0,
                LastLoggedDate = DateOnly.MinValue
            });
        }

        return streaks.Select(s => new StreakResponse(
            s.StreakType.ToString(),
            s.CurrentCount,
            s.LongestCount,
            s.LastLoggedDate
        )).ToList();
    }

    private async Task UpdateStreak(Guid userId, StreakType type, bool achieved, DateOnly logDate)
    {
        var streak = await _db.Streaks
            .FirstOrDefaultAsync(s => s.UserId == userId && s.StreakType == type);

        if (streak == null)
        {
            streak = new Streak
            {
                UserId = userId,
                StreakType = type,
                CurrentCount = 0,
                LongestCount = 0,
                LastLoggedDate = DateOnly.MinValue
            };
            _db.Streaks.Add(streak);
        }

        if (!achieved)
        {
            if (streak.CurrentCount > 0)
                _logger.LogInformation("Streak broken — Type: {StreakType}, Was: {Count} days, User: {UserId}",
                    type, streak.CurrentCount, userId.ToString()[..8]);
            streak.CurrentCount = 0;
        }
        else if (streak.LastLoggedDate == logDate)
        {
            // Same-day re-log: streak already counted, no change needed
            return;
        }
        else
        {
            var yesterday = logDate.AddDays(-1);
            streak.CurrentCount = streak.LastLoggedDate == yesterday
                ? streak.CurrentCount + 1
                : 1;

            if (streak.CurrentCount > streak.LongestCount)
            {
                streak.LongestCount = streak.CurrentCount;
                _logger.LogInformation("New streak record — Type: {StreakType}, Count: {Count}, User: {UserId}",
                    type, streak.CurrentCount, userId.ToString()[..8]);
            }
        }

        streak.LastLoggedDate = logDate;
        streak.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
