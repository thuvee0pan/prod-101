using ExecutionOS.API.Data;
using ExecutionOS.API.Jobs;
using ExecutionOS.API.Models;
using ExecutionOS.API.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExecutionOS.Tests.Jobs;

public class InactivityDetectionJobTests
{
    private readonly Guid _userId = Guid.NewGuid();

    private (InactivityDetectionJob job, AppDbContext db) CreateJob()
    {
        var db = TestDbHelper.CreateInMemoryDb();

        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddScoped<AppDbContext>(_ => db);
        services.AddScoped<WarningService>();
        var serviceProvider = services.BuildServiceProvider();

        var logger = new Mock<ILogger<InactivityDetectionJob>>();
        var job = new InactivityDetectionJob(serviceProvider, logger.Object);

        return (job, db);
    }

    [Fact]
    public async Task NoLogs_CreatesInactivityWarning()
    {
        var (job, db) = CreateJob();

        // Seed a user with no logs
        db.Users.Add(new User { Id = _userId, Email = "test@test.com", Name = "Test" });
        await db.SaveChangesAsync();

        // Execute by calling ExecuteAsync indirectly via reflection or just test the logic
        // Since ExecuteAsync is protected, we test via the hosted service pattern
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var task = job.StartAsync(cts.Token);

        // Give it time to run one cycle
        await Task.Delay(1000);
        await job.StopAsync(CancellationToken.None);

        var warnings = db.InactivityWarnings.Where(w => w.UserId == _userId).ToList();
        Assert.Single(warnings);
        Assert.Equal("no_daily_log", warnings[0].WarningType);
        Assert.Contains("never logged", warnings[0].Message);
    }

    [Fact]
    public async Task RecentLog_NoWarning()
    {
        var (job, db) = CreateJob();

        db.Users.Add(new User { Id = _userId, Email = "test@test.com", Name = "Test" });
        db.DailyLogs.Add(new DailyLog
        {
            UserId = _userId,
            LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DeepWorkMinutes = 120,
            GymCompleted = true,
            LearningMinutes = 30,
            AlcoholFree = true
        });
        await db.SaveChangesAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var task = job.StartAsync(cts.Token);
        await Task.Delay(1000);
        await job.StopAsync(CancellationToken.None);

        var warnings = db.InactivityWarnings.Where(w => w.UserId == _userId).ToList();
        Assert.Empty(warnings);
    }

    [Fact]
    public async Task StaleLog_CreatesWarning()
    {
        var (job, db) = CreateJob();

        db.Users.Add(new User { Id = _userId, Email = "test@test.com", Name = "Test" });
        db.DailyLogs.Add(new DailyLog
        {
            UserId = _userId,
            LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            DeepWorkMinutes = 120,
            GymCompleted = true,
            LearningMinutes = 30,
            AlcoholFree = true
        });
        await db.SaveChangesAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var task = job.StartAsync(cts.Token);
        await Task.Delay(1000);
        await job.StopAsync(CancellationToken.None);

        var warnings = db.InactivityWarnings.Where(w => w.UserId == _userId).ToList();
        Assert.Contains(warnings, w => w.WarningType == "no_daily_log");
    }

    [Fact]
    public async Task DuplicateWarning_Suppressed()
    {
        var (job, db) = CreateJob();

        db.Users.Add(new User { Id = _userId, Email = "test@test.com", Name = "Test" });
        // Pre-existing recent warning
        db.InactivityWarnings.Add(new InactivityWarning
        {
            UserId = _userId,
            WarningType = "no_daily_log",
            Message = "Already warned",
            TriggeredAt = DateTime.UtcNow.AddDays(-1) // within 7-day window
        });
        await db.SaveChangesAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var task = job.StartAsync(cts.Token);
        await Task.Delay(1000);
        await job.StopAsync(CancellationToken.None);

        var warnings = db.InactivityWarnings.Where(w => w.UserId == _userId).ToList();
        // Should still be just the original one
        Assert.Single(warnings);
    }

    [Fact]
    public async Task StaleProject_CreatesWarning()
    {
        var (job, db) = CreateJob();

        db.Users.Add(new User { Id = _userId, Email = "test@test.com", Name = "Test" });
        db.Projects.Add(new Project
        {
            UserId = _userId,
            Title = "My Stale App",
            Description = "No progress",
            Status = ProjectStatus.Active
        });
        // Recent log but without mentioning the project
        db.DailyLogs.Add(new DailyLog
        {
            UserId = _userId,
            LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DeepWorkMinutes = 120,
            GymCompleted = true,
            LearningMinutes = 30,
            AlcoholFree = true,
            Notes = "Worked on something else entirely"
        });
        await db.SaveChangesAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var task = job.StartAsync(cts.Token);
        await Task.Delay(1000);
        await job.StopAsync(CancellationToken.None);

        var warnings = db.InactivityWarnings
            .Where(w => w.UserId == _userId && w.WarningType == "stale_project")
            .ToList();
        Assert.Single(warnings);
        Assert.Contains("My Stale App", warnings[0].Message);
    }

    [Fact]
    public async Task ActiveProject_MentionedInNotes_NoWarning()
    {
        var (job, db) = CreateJob();

        db.Users.Add(new User { Id = _userId, Email = "test@test.com", Name = "Test" });
        db.Projects.Add(new Project
        {
            UserId = _userId,
            Title = "ExecutionOS",
            Description = "Build the app",
            Status = ProjectStatus.Active
        });
        db.DailyLogs.Add(new DailyLog
        {
            UserId = _userId,
            LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DeepWorkMinutes = 120,
            GymCompleted = true,
            LearningMinutes = 30,
            AlcoholFree = true,
            Notes = "Made progress on ExecutionOS today"
        });
        await db.SaveChangesAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var task = job.StartAsync(cts.Token);
        await Task.Delay(1000);
        await job.StopAsync(CancellationToken.None);

        var warnings = db.InactivityWarnings
            .Where(w => w.UserId == _userId && w.WarningType == "stale_project")
            .ToList();
        Assert.Empty(warnings);
    }
}
