using ExecutionOS.API.DTOs;
using ExecutionOS.API.Models;
using ExecutionOS.API.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ExecutionOS.Tests.Services;

public class GoalServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task CreateGoal_FirstGoal_Succeeds()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new GoalService(db, NullLogger<GoalService>.Instance);

        var result = await service.CreateGoal(_userId, new CreateGoalRequest("Ship MVP", "Launch in 90 days"));

        Assert.Equal("Ship MVP", result.Title);
        Assert.Equal("Active", result.Status);
        Assert.True(result.DaysRemaining > 0);
    }

    [Fact]
    public async Task CreateGoal_AlreadyHasActiveGoal_Throws()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new GoalService(db, NullLogger<GoalService>.Instance);

        await service.CreateGoal(_userId, new CreateGoalRequest("Goal 1", "First"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateGoal(_userId, new CreateGoalRequest("Goal 2", "Second")));

        Assert.Contains("already have an active 90-day goal", ex.Message);
    }

    [Fact]
    public async Task CreateGoal_AfterCompleting_AllowsNew()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new GoalService(db, NullLogger<GoalService>.Instance);

        var goal1 = await service.CreateGoal(_userId, new CreateGoalRequest("Goal 1", "First"));
        await service.CompleteGoal(_userId, goal1.Id);

        var goal2 = await service.CreateGoal(_userId, new CreateGoalRequest("Goal 2", "Second"));

        Assert.Equal("Goal 2", goal2.Title);
        Assert.Equal("Active", goal2.Status);
    }

    [Fact]
    public async Task CompleteGoal_SetsStatusToCompleted()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new GoalService(db, NullLogger<GoalService>.Instance);

        var goal = await service.CreateGoal(_userId, new CreateGoalRequest("My Goal", "Description"));
        var completed = await service.CompleteGoal(_userId, goal.Id);

        Assert.Equal("Completed", completed.Status);
    }

    [Fact]
    public async Task CompleteGoal_NonExistent_Throws()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new GoalService(db, NullLogger<GoalService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CompleteGoal(_userId, Guid.NewGuid()));
    }

    [Fact]
    public async Task AbandonGoal_SetsStatusAndReason()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new GoalService(db, NullLogger<GoalService>.Instance);

        var goal = await service.CreateGoal(_userId, new CreateGoalRequest("My Goal", "Description"));
        var abandoned = await service.AbandonGoal(_userId, goal.Id, new AbandonGoalRequest("Changed direction"));

        Assert.Equal("Abandoned", abandoned.Status);
    }

    [Fact]
    public async Task GetActiveGoal_NoGoals_ReturnsNull()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new GoalService(db, NullLogger<GoalService>.Instance);

        var result = await service.GetActiveGoal(_userId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllGoals_ReturnsAllStatuses()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new GoalService(db, NullLogger<GoalService>.Instance);

        var goal1 = await service.CreateGoal(_userId, new CreateGoalRequest("Goal 1", "First"));
        await service.AbandonGoal(_userId, goal1.Id, new AbandonGoalRequest("Bad idea"));
        await service.CreateGoal(_userId, new CreateGoalRequest("Goal 2", "Second"));

        var goals = await service.GetAllGoals(_userId);

        Assert.Equal(2, goals.Count);
    }

    [Fact]
    public async Task GetActiveGoal_DifferentUsers_Isolated()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new GoalService(db, NullLogger<GoalService>.Instance);
        var userId2 = Guid.NewGuid();

        await service.CreateGoal(_userId, new CreateGoalRequest("User1 Goal", "First"));
        await service.CreateGoal(userId2, new CreateGoalRequest("User2 Goal", "Second"));

        var user1Goal = await service.GetActiveGoal(_userId);
        var user2Goal = await service.GetActiveGoal(userId2);

        Assert.Equal("User1 Goal", user1Goal!.Title);
        Assert.Equal("User2 Goal", user2Goal!.Title);
    }
}
