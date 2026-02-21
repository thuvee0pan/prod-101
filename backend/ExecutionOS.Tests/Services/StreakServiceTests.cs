using ExecutionOS.API.Models;
using ExecutionOS.API.Services;
using Xunit;

namespace ExecutionOS.Tests.Services;

public class StreakServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task UpdateStreaks_FirstLog_CreatesStreakWithCount1()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new StreakService(db);
        var log = CreateLog(deepWork: 120, gym: true, learning: 30, sober: true,
            date: new DateOnly(2024, 6, 10));

        await service.UpdateStreaks(_userId, log);

        var streaks = await service.GetStreaks(_userId);
        Assert.All(streaks, s => Assert.Equal(1, s.CurrentCount));
    }

    [Fact]
    public async Task UpdateStreaks_ConsecutiveDays_IncrementsStreak()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new StreakService(db);

        var day1 = CreateLog(deepWork: 120, gym: true, learning: 30, sober: true,
            date: new DateOnly(2024, 6, 10));
        var day2 = CreateLog(deepWork: 120, gym: true, learning: 30, sober: true,
            date: new DateOnly(2024, 6, 11));

        await service.UpdateStreaks(_userId, day1);
        await service.UpdateStreaks(_userId, day2);

        var streaks = await service.GetStreaks(_userId);
        Assert.All(streaks, s => Assert.Equal(2, s.CurrentCount));
    }

    [Fact]
    public async Task UpdateStreaks_MissedDay_ResetsStreakTo1()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new StreakService(db);

        var day1 = CreateLog(deepWork: 120, gym: true, learning: 30, sober: true,
            date: new DateOnly(2024, 6, 10));
        // Skip June 11
        var day3 = CreateLog(deepWork: 120, gym: true, learning: 30, sober: true,
            date: new DateOnly(2024, 6, 12));

        await service.UpdateStreaks(_userId, day1);
        await service.UpdateStreaks(_userId, day3);

        var streaks = await service.GetStreaks(_userId);
        Assert.All(streaks, s => Assert.Equal(1, s.CurrentCount));
    }

    [Fact]
    public async Task UpdateStreaks_NotAchieved_ResetsToZero()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new StreakService(db);

        var day1 = CreateLog(deepWork: 120, gym: true, learning: 30, sober: true,
            date: new DateOnly(2024, 6, 10));
        var day2 = CreateLog(deepWork: 0, gym: false, learning: 0, sober: false,
            date: new DateOnly(2024, 6, 11));

        await service.UpdateStreaks(_userId, day1);
        await service.UpdateStreaks(_userId, day2);

        var streaks = await service.GetStreaks(_userId);
        Assert.All(streaks, s => Assert.Equal(0, s.CurrentCount));
    }

    [Fact]
    public async Task UpdateStreaks_SameDayReLog_DoesNotDoubleIncrement()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new StreakService(db);

        var day1 = CreateLog(deepWork: 120, gym: true, learning: 30, sober: true,
            date: new DateOnly(2024, 6, 10));
        var day2 = CreateLog(deepWork: 120, gym: true, learning: 30, sober: true,
            date: new DateOnly(2024, 6, 11));
        // Re-log same day
        var day2Again = CreateLog(deepWork: 180, gym: true, learning: 60, sober: true,
            date: new DateOnly(2024, 6, 11));

        await service.UpdateStreaks(_userId, day1);
        await service.UpdateStreaks(_userId, day2);
        await service.UpdateStreaks(_userId, day2Again);

        var streaks = await service.GetStreaks(_userId);
        // Should still be 2, not 3
        Assert.All(streaks, s => Assert.Equal(2, s.CurrentCount));
    }

    [Fact]
    public async Task UpdateStreaks_LongestCountTracked()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new StreakService(db);

        // Build a 3-day streak
        for (int i = 0; i < 3; i++)
        {
            var log = CreateLog(deepWork: 120, gym: true, learning: 30, sober: true,
                date: new DateOnly(2024, 6, 10 + i));
            await service.UpdateStreaks(_userId, log);
        }

        // Break it
        var breakLog = CreateLog(deepWork: 0, gym: false, learning: 0, sober: false,
            date: new DateOnly(2024, 6, 13));
        await service.UpdateStreaks(_userId, breakLog);

        // Start a new 1-day streak
        var newLog = CreateLog(deepWork: 120, gym: true, learning: 30, sober: true,
            date: new DateOnly(2024, 6, 14));
        await service.UpdateStreaks(_userId, newLog);

        var streaks = await service.GetStreaks(_userId);
        Assert.All(streaks, s =>
        {
            Assert.Equal(1, s.CurrentCount);
            Assert.Equal(3, s.LongestCount);
        });
    }

    [Fact]
    public async Task UpdateStreaks_PartialAchievement_OnlyAffectsRelevantStreaks()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new StreakService(db);

        // Day 1: all achieved
        var day1 = CreateLog(deepWork: 120, gym: true, learning: 30, sober: true,
            date: new DateOnly(2024, 6, 10));
        // Day 2: only gym and sober
        var day2 = CreateLog(deepWork: 0, gym: true, learning: 0, sober: true,
            date: new DateOnly(2024, 6, 11));

        await service.UpdateStreaks(_userId, day1);
        await service.UpdateStreaks(_userId, day2);

        var streaks = await service.GetStreaks(_userId);
        var gymStreak = streaks.First(s => s.StreakType == "Gym");
        var deepWorkStreak = streaks.First(s => s.StreakType == "DeepWork");

        Assert.Equal(2, gymStreak.CurrentCount);
        Assert.Equal(0, deepWorkStreak.CurrentCount);
    }

    [Fact]
    public async Task GetStreaks_NoData_ReturnsAllTypesWithZeroCounts()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new StreakService(db);

        var streaks = await service.GetStreaks(_userId);

        Assert.Equal(4, streaks.Count);
        Assert.All(streaks, s =>
        {
            Assert.Equal(0, s.CurrentCount);
            Assert.Equal(0, s.LongestCount);
        });
    }

    private DailyLog CreateLog(int deepWork, bool gym, int learning, bool sober, DateOnly date) =>
        new()
        {
            UserId = _userId,
            LogDate = date,
            DeepWorkMinutes = deepWork,
            GymCompleted = gym,
            LearningMinutes = learning,
            AlcoholFree = sober
        };
}
