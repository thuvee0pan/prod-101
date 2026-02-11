using ExecutionOS.API.Data;
using ExecutionOS.API.DTOs;
using ExecutionOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExecutionOS.API.Services;

public class DailyLogService
{
    private readonly AppDbContext _db;
    private readonly StreakService _streaks;

    public DailyLogService(AppDbContext db, StreakService streaks)
    {
        _db = db;
        _streaks = streaks;
    }

    public async Task<DailyLogResponse> LogToday(Guid userId, CreateDailyLogRequest request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var existing = await _db.DailyLogs
            .FirstOrDefaultAsync(l => l.UserId == userId && l.LogDate == today);

        if (existing != null)
        {
            existing.DeepWorkMinutes = request.DeepWorkMinutes;
            existing.GymCompleted = request.GymCompleted;
            existing.LearningMinutes = request.LearningMinutes;
            existing.AlcoholFree = request.AlcoholFree;
            existing.Notes = request.Notes;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new DailyLog
            {
                UserId = userId,
                LogDate = today,
                DeepWorkMinutes = request.DeepWorkMinutes,
                GymCompleted = request.GymCompleted,
                LearningMinutes = request.LearningMinutes,
                AlcoholFree = request.AlcoholFree,
                Notes = request.Notes
            };
            _db.DailyLogs.Add(existing);
        }

        await _db.SaveChangesAsync();

        await _streaks.UpdateStreaks(userId, existing);

        return MapToResponse(existing);
    }

    public async Task<DailyLogResponse?> GetToday(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var log = await _db.DailyLogs
            .FirstOrDefaultAsync(l => l.UserId == userId && l.LogDate == today);

        return log == null ? null : MapToResponse(log);
    }

    public async Task<List<DailyLogResponse>> GetLogs(Guid userId, DateOnly from, DateOnly to)
    {
        var logs = await _db.DailyLogs
            .Where(l => l.UserId == userId && l.LogDate >= from && l.LogDate <= to)
            .OrderByDescending(l => l.LogDate)
            .ToListAsync();

        return logs.Select(MapToResponse).ToList();
    }

    private static DailyLogResponse MapToResponse(DailyLog log) =>
        new(log.Id, log.LogDate, log.DeepWorkMinutes, log.GymCompleted,
            log.LearningMinutes, log.AlcoholFree, log.Notes, log.CreatedAt);
}
