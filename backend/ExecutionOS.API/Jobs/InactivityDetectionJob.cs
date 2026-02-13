using ExecutionOS.API.Data;
using ExecutionOS.API.Models;
using ExecutionOS.API.Services;
using Microsoft.EntityFrameworkCore;

namespace ExecutionOS.API.Jobs;

public class InactivityDetectionJob : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<InactivityDetectionJob> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);
    private const int InactivityThresholdDays = 7;

    public InactivityDetectionJob(IServiceProvider services, ILogger<InactivityDetectionJob> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckInactivity(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during inactivity detection");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckInactivity(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var warningService = scope.ServiceProvider.GetRequiredService<WarningService>();

        var users = await db.Users.ToListAsync(ct);
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-InactivityThresholdDays));

        foreach (var user in users)
        {
            // Check for no daily logs in 7 days
            var lastLog = await db.DailyLogs
                .Where(l => l.UserId == user.Id)
                .OrderByDescending(l => l.LogDate)
                .FirstOrDefaultAsync(ct);

            if (lastLog == null || lastLog.LogDate < cutoff)
            {
                var daysSince = lastLog == null
                    ? "never logged"
                    : $"{(DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - lastLog.LogDate.DayNumber)} days since last log";

                // Check if we already warned recently
                var recentWarning = await db.InactivityWarnings
                    .AnyAsync(w => w.UserId == user.Id
                        && w.WarningType == "no_daily_log"
                        && w.TriggeredAt > DateTime.UtcNow.AddDays(-InactivityThresholdDays), ct);

                if (!recentWarning)
                {
                    await warningService.CreateWarning(
                        user.Id,
                        "no_daily_log",
                        $"You haven't logged your daily execution in over 7 days ({daysSince}). " +
                        "Consistency is the only thing that compounds. Log today or acknowledge this warning."
                    );
                    _logger.LogInformation("Inactivity warning created for user {UserId}", user.Id);
                }
            }

            // Check active projects with no mention in notes
            var activeProjects = await db.Projects
                .Where(p => p.UserId == user.Id && p.Status == ProjectStatus.Active)
                .ToListAsync(ct);

            foreach (var project in activeProjects)
            {
                var recentMention = await db.DailyLogs
                    .AnyAsync(l => l.UserId == user.Id
                        && l.LogDate >= cutoff
                        && l.Notes != null
                        && l.Notes.Contains(project.Title), ct);

                if (!recentMention)
                {
                    var alreadyWarned = await db.InactivityWarnings
                        .AnyAsync(w => w.UserId == user.Id
                            && w.WarningType == "stale_project"
                            && w.Message.Contains(project.Title)
                            && w.TriggeredAt > DateTime.UtcNow.AddDays(-InactivityThresholdDays), ct);

                    if (!alreadyWarned)
                    {
                        await warningService.CreateWarning(
                            user.Id,
                            "stale_project",
                            $"Project '{project.Title}' has had no progress mentions in the last 7 days. " +
                            "Either work on it or drop it. Carrying dead projects is a form of self-deception."
                        );
                    }
                }
            }
        }
    }
}
