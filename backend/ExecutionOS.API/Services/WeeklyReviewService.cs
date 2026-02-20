using ExecutionOS.API.Data;
using ExecutionOS.API.DTOs;
using ExecutionOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExecutionOS.API.Services;

public class WeeklyReviewService
{
    private readonly AppDbContext _db;
    private readonly AiService _ai;

    public WeeklyReviewService(AppDbContext db, AiService ai)
    {
        _db = db;
        _ai = ai;
    }

    public async Task<WeeklyReviewResponse> Generate(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        // DayOfWeek: Sunday=0 .. Saturday=6. We need Monday as start.
        var daysFromMonday = ((int)today.DayOfWeek + 6) % 7; // Monday=0, Sunday=6
        var weekStart = today.AddDays(-daysFromMonday);
        var weekEnd = weekStart.AddDays(6); // Sunday

        // Gather data
        var logs = await _db.DailyLogs
            .Where(l => l.UserId == userId && l.LogDate >= weekStart && l.LogDate <= weekEnd)
            .OrderBy(l => l.LogDate)
            .ToListAsync();

        var goal = await _db.Goals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.Status == GoalStatus.Active);

        var projects = await _db.Projects
            .Where(p => p.UserId == userId && p.Status == ProjectStatus.Active)
            .ToListAsync();

        var streaks = await _db.Streaks
            .Where(s => s.UserId == userId)
            .ToListAsync();

        // Build summary
        var logSummary = string.Join("\n", logs.Select(l =>
            $"- {l.LogDate}: Deep Work={l.DeepWorkMinutes}min, Gym={l.GymCompleted}, " +
            $"Learning={l.LearningMinutes}min, Sober={l.AlcoholFree}" +
            (string.IsNullOrEmpty(l.Notes) ? "" : $" | Notes: {l.Notes}")
        ));

        if (string.IsNullOrEmpty(logSummary))
            logSummary = "No daily logs recorded this week.";

        var context = new WeeklyReviewContext
        {
            GoalTitle = goal?.Title ?? "No active goal",
            GoalDayNumber = goal != null ? (int)(DateTime.UtcNow - goal.StartDate).TotalDays : 0,
            ActiveProjects = projects.Select(p => p.Title).ToList(),
            DailyLogsSummary = logSummary,
            DeepWorkStreak = streaks.FirstOrDefault(s => s.StreakType == StreakType.DeepWork)?.CurrentCount ?? 0,
            GymStreak = streaks.FirstOrDefault(s => s.StreakType == StreakType.Gym)?.CurrentCount ?? 0,
            LearningStreak = streaks.FirstOrDefault(s => s.StreakType == StreakType.Learning)?.CurrentCount ?? 0,
            SoberStreak = streaks.FirstOrDefault(s => s.StreakType == StreakType.Sober)?.CurrentCount ?? 0
        };

        var result = await _ai.GenerateWeeklyReview(context);

        var review = new WeeklyReview
        {
            UserId = userId,
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            WhatWorked = result.WhatWorked,
            WhereAvoided = result.WhereAvoided,
            WhatToCut = result.WhatToCut,
            AiSummary = result.FullResponse
        };

        _db.WeeklyReviews.Add(review);
        await _db.SaveChangesAsync();

        return MapToResponse(review);
    }

    public async Task<List<WeeklyReviewResponse>> GetAll(Guid userId)
    {
        var reviews = await _db.WeeklyReviews
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync();

        return reviews.Select(MapToResponse).ToList();
    }

    public async Task<WeeklyReviewResponse?> GetLatest(Guid userId)
    {
        var review = await _db.WeeklyReviews
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.GeneratedAt)
            .FirstOrDefaultAsync();

        return review == null ? null : MapToResponse(review);
    }

    private static WeeklyReviewResponse MapToResponse(WeeklyReview r) =>
        new(r.Id, r.WeekStart, r.WeekEnd, r.WhatWorked, r.WhereAvoided, r.WhatToCut, r.AiSummary, r.GeneratedAt);
}
