using ExecutionOS.API.Services;
using Xunit;

namespace ExecutionOS.Tests.Services;

public class WarningServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task CreateWarning_AddsToDb()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new WarningService(db);

        await service.CreateWarning(_userId, "no_daily_log", "You haven't logged in 7 days.");

        var warnings = await service.GetActiveWarnings(_userId);
        Assert.Single(warnings);
        Assert.Equal("no_daily_log", warnings[0].WarningType);
        Assert.False(warnings[0].Acknowledged);
    }

    [Fact]
    public async Task AcknowledgeWarning_MarksAcknowledged()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new WarningService(db);

        await service.CreateWarning(_userId, "no_daily_log", "Log your day.");
        var warnings = await service.GetActiveWarnings(_userId);

        await service.AcknowledgeWarning(_userId, warnings[0].Id);

        var active = await service.GetActiveWarnings(_userId);
        Assert.Empty(active);
    }

    [Fact]
    public async Task AcknowledgeWarning_WrongUser_Throws()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new WarningService(db);

        await service.CreateWarning(_userId, "no_daily_log", "Log your day.");
        var warnings = await service.GetActiveWarnings(_userId);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AcknowledgeWarning(Guid.NewGuid(), warnings[0].Id));
    }

    [Fact]
    public async Task AcknowledgeWarning_NonExistent_Throws()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new WarningService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AcknowledgeWarning(_userId, Guid.NewGuid()));
    }

    [Fact]
    public async Task GetActiveWarnings_ExcludesAcknowledged()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new WarningService(db);

        await service.CreateWarning(_userId, "no_daily_log", "Warning 1");
        await service.CreateWarning(_userId, "stale_project", "Warning 2");

        var all = await service.GetActiveWarnings(_userId);
        var noDailyLogWarning = all.First(w => w.WarningType == "no_daily_log");
        await service.AcknowledgeWarning(_userId, noDailyLogWarning.Id);

        var remaining = await service.GetActiveWarnings(_userId);
        Assert.Single(remaining);
        Assert.Equal("stale_project", remaining[0].WarningType);
    }

    [Fact]
    public async Task GetActiveWarnings_DifferentUsers_Isolated()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new WarningService(db);
        var userId2 = Guid.NewGuid();

        await service.CreateWarning(_userId, "no_daily_log", "User 1 warning");
        await service.CreateWarning(userId2, "stale_project", "User 2 warning");

        var user1Warnings = await service.GetActiveWarnings(_userId);
        var user2Warnings = await service.GetActiveWarnings(userId2);

        Assert.Single(user1Warnings);
        Assert.Equal("no_daily_log", user1Warnings[0].WarningType);
        Assert.Single(user2Warnings);
        Assert.Equal("stale_project", user2Warnings[0].WarningType);
    }
}
