using ExecutionOS.API.DTOs;
using ExecutionOS.API.Services;

namespace ExecutionOS.Tests.Services;

public class DailyLogServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task LogToday_FirstLog_CreatesNew()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var streakService = new StreakService(db);
        var service = new DailyLogService(db, streakService);

        var result = await service.LogToday(_userId, new CreateDailyLogRequest(
            120, true, 30, true, "Productive day"));

        Assert.Equal(120, result.DeepWorkMinutes);
        Assert.True(result.GymCompleted);
        Assert.Equal(30, result.LearningMinutes);
        Assert.True(result.AlcoholFree);
        Assert.Equal("Productive day", result.Notes);
    }

    [Fact]
    public async Task LogToday_SecondLog_UpdatesExisting()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var streakService = new StreakService(db);
        var service = new DailyLogService(db, streakService);

        var first = await service.LogToday(_userId, new CreateDailyLogRequest(
            60, false, 15, true, "Morning log"));

        var second = await service.LogToday(_userId, new CreateDailyLogRequest(
            180, true, 45, true, "Updated log"));

        // Same record, updated values
        Assert.Equal(first.Id, second.Id);
        Assert.Equal(180, second.DeepWorkMinutes);
        Assert.True(second.GymCompleted);
        Assert.Equal("Updated log", second.Notes);
    }

    [Fact]
    public async Task GetToday_NoLog_ReturnsNull()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var streakService = new StreakService(db);
        var service = new DailyLogService(db, streakService);

        var result = await service.GetToday(_userId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetToday_AfterLogging_ReturnsLog()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var streakService = new StreakService(db);
        var service = new DailyLogService(db, streakService);

        await service.LogToday(_userId, new CreateDailyLogRequest(120, true, 30, true, null));
        var result = await service.GetToday(_userId);

        Assert.NotNull(result);
        Assert.Equal(120, result.DeepWorkMinutes);
    }

    [Fact]
    public async Task GetLogs_DateRange_FiltersCorrectly()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var streakService = new StreakService(db);
        var service = new DailyLogService(db, streakService);

        // Create logs by directly adding to DB (to control dates)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.DailyLogs.Add(new API.Models.DailyLog
        {
            UserId = _userId,
            LogDate = today.AddDays(-5),
            DeepWorkMinutes = 100,
            GymCompleted = true,
            LearningMinutes = 30,
            AlcoholFree = true
        });
        db.DailyLogs.Add(new API.Models.DailyLog
        {
            UserId = _userId,
            LogDate = today.AddDays(-2),
            DeepWorkMinutes = 200,
            GymCompleted = false,
            LearningMinutes = 60,
            AlcoholFree = true
        });
        db.DailyLogs.Add(new API.Models.DailyLog
        {
            UserId = _userId,
            LogDate = today.AddDays(-10),
            DeepWorkMinutes = 50,
            GymCompleted = true,
            LearningMinutes = 15,
            AlcoholFree = false
        });
        await db.SaveChangesAsync();

        var logs = await service.GetLogs(_userId, today.AddDays(-7), today);

        Assert.Equal(2, logs.Count);
    }

    [Fact]
    public async Task LogToday_TriggersStreakUpdate()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var streakService = new StreakService(db);
        var service = new DailyLogService(db, streakService);

        await service.LogToday(_userId, new CreateDailyLogRequest(120, true, 30, true, null));

        var streaks = await streakService.GetStreaks(_userId);
        Assert.Contains(streaks, s => s.StreakType == "DeepWork" && s.CurrentCount == 1);
        Assert.Contains(streaks, s => s.StreakType == "Gym" && s.CurrentCount == 1);
    }

    [Fact]
    public async Task GetLogs_DifferentUsers_Isolated()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var streakService = new StreakService(db);
        var service = new DailyLogService(db, streakService);
        var userId2 = Guid.NewGuid();

        await service.LogToday(_userId, new CreateDailyLogRequest(120, true, 30, true, null));
        await service.LogToday(userId2, new CreateDailyLogRequest(60, false, 15, false, null));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var user1Logs = await service.GetLogs(_userId, today, today);
        var user2Logs = await service.GetLogs(userId2, today, today);

        Assert.Single(user1Logs);
        Assert.Equal(120, user1Logs[0].DeepWorkMinutes);
        Assert.Single(user2Logs);
        Assert.Equal(60, user2Logs[0].DeepWorkMinutes);
    }
}
