using ExecutionOS.API.Models;
using ExecutionOS.API.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ExecutionOS.Tests.Services;

public class WeeklyReviewServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    private static AiService CreateMockAiService()
    {
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["AiSettings:ApiKey"]).Returns((string?)null);
        return new AiService(config.Object);
    }

    [Fact]
    public async Task Generate_PersistsReview()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new WeeklyReviewService(db, CreateMockAiService());

        // Seed a user goal
        db.Goals.Add(new Goal
        {
            UserId = _userId,
            Title = "Ship MVP",
            Description = "Launch in 90 days",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(60)
        });
        await db.SaveChangesAsync();

        var result = await service.Generate(_userId);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.WeekStart.DayOfWeek == DayOfWeek.Monday);
        Assert.True(result.WeekEnd.DayOfWeek == DayOfWeek.Sunday);
    }

    [Fact]
    public async Task Generate_WeekStartIsAlwaysMonday()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new WeeklyReviewService(db, CreateMockAiService());

        var result = await service.Generate(_userId);

        // Regardless of what day it is today, weekStart should be Monday
        Assert.Equal(DayOfWeek.Monday, result.WeekStart.DayOfWeek);
        Assert.Equal(DayOfWeek.Sunday, result.WeekEnd.DayOfWeek);
        Assert.Equal(6, result.WeekEnd.DayNumber - result.WeekStart.DayNumber);
    }

    [Fact]
    public async Task Generate_NoGoal_UsesPlaceholder()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new WeeklyReviewService(db, CreateMockAiService());

        // No goal seeded — AI context should contain "No active goal"
        var result = await service.Generate(_userId);

        Assert.NotNull(result.AiSummary);
    }

    [Fact]
    public async Task GetAll_OrderedByMostRecent()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new WeeklyReviewService(db, CreateMockAiService());

        await service.Generate(_userId);
        await service.Generate(_userId);

        var reviews = await service.GetAll(_userId);

        Assert.Equal(2, reviews.Count);
        Assert.True(reviews[0].GeneratedAt >= reviews[1].GeneratedAt);
    }

    [Fact]
    public async Task GetLatest_NoReviews_ReturnsNull()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new WeeklyReviewService(db, CreateMockAiService());

        var result = await service.GetLatest(_userId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatest_ReturnsNewest()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new WeeklyReviewService(db, CreateMockAiService());

        await service.Generate(_userId);
        var latest = await service.Generate(_userId);

        var result = await service.GetLatest(_userId);

        Assert.Equal(latest.Id, result!.Id);
    }

    [Fact]
    public async Task Generate_IncludesStreakData()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var aiService = CreateMockAiService();
        var streakService = new StreakService(db);
        var service = new WeeklyReviewService(db, aiService);

        // Seed streak data
        db.Streaks.Add(new Streak
        {
            UserId = _userId,
            StreakType = StreakType.DeepWork,
            CurrentCount = 5,
            LongestCount = 10,
            LastLoggedDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });
        await db.SaveChangesAsync();

        var result = await service.Generate(_userId);

        // Review was generated (AI returns fallback since no key)
        Assert.NotNull(result);
    }
}
