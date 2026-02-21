using ExecutionOS.API.DTOs;
using ExecutionOS.API.Models;
using ExecutionOS.API.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ExecutionOS.Tests.Services;

public class ProjectServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    private static AiService CreateMockAiService()
    {
        var config = new Mock<IConfiguration>();
        // No API key means AI returns fallback string
        config.Setup(c => c["AiSettings:ApiKey"]).Returns((string?)null);
        return new AiService(config.Object);
    }

    [Fact]
    public async Task CreateProject_FirstProject_Succeeds()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new ProjectService(db, CreateMockAiService());

        var result = await service.CreateProject(_userId,
            new CreateProjectRequest("My App", "A cool app", null));

        Assert.Equal("My App", result.Title);
        Assert.Equal("Active", result.Status);
    }

    [Fact]
    public async Task CreateProject_TwoProjects_Succeeds()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new ProjectService(db, CreateMockAiService());

        await service.CreateProject(_userId, new CreateProjectRequest("App 1", "First", null));
        var second = await service.CreateProject(_userId, new CreateProjectRequest("App 2", "Second", null));

        Assert.Equal("App 2", second.Title);
    }

    [Fact]
    public async Task CreateProject_ThirdProject_ThrowsMaxActiveLimit()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new ProjectService(db, CreateMockAiService());

        await service.CreateProject(_userId, new CreateProjectRequest("App 1", "First", null));
        await service.CreateProject(_userId, new CreateProjectRequest("App 2", "Second", null));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateProject(_userId, new CreateProjectRequest("App 3", "Third", null)));

        Assert.Contains("2 active projects", ex.Message);
    }

    [Fact]
    public async Task CreateProject_AfterDropping_AllowsNew()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new ProjectService(db, CreateMockAiService());

        var p1 = await service.CreateProject(_userId, new CreateProjectRequest("App 1", "First", null));
        await service.CreateProject(_userId, new CreateProjectRequest("App 2", "Second", null));
        await service.UpdateStatus(_userId, p1.Id, new UpdateProjectStatusRequest("Dropped"));

        var p3 = await service.CreateProject(_userId, new CreateProjectRequest("App 3", "Third", null));
        Assert.Equal("Active", p3.Status);
    }

    [Fact]
    public async Task UpdateStatus_ValidStatus_Updates()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new ProjectService(db, CreateMockAiService());

        var project = await service.CreateProject(_userId, new CreateProjectRequest("App", "Desc", null));
        var updated = await service.UpdateStatus(_userId, project.Id, new UpdateProjectStatusRequest("Paused"));

        Assert.Equal("Paused", updated.Status);
    }

    [Fact]
    public async Task UpdateStatus_InvalidStatus_Throws()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new ProjectService(db, CreateMockAiService());

        var project = await service.CreateProject(_userId, new CreateProjectRequest("App", "Desc", null));

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateStatus(_userId, project.Id, new UpdateProjectStatusRequest("InvalidStatus")));
    }

    [Fact]
    public async Task UpdateStatus_NonExistentProject_Throws()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new ProjectService(db, CreateMockAiService());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateStatus(_userId, Guid.NewGuid(), new UpdateProjectStatusRequest("Paused")));
    }

    [Fact]
    public async Task SubmitChangeRequest_ShortJustification_Throws()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new ProjectService(db, CreateMockAiService());

        var project = await service.CreateProject(_userId, new CreateProjectRequest("App", "Desc", null));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitChangeRequest(_userId, new ProjectChangeRequestDto(
                "New App", "New desc", "Too short", project.Id)));

        Assert.Contains("at least 50 characters", ex.Message);
    }

    [Fact]
    public async Task SubmitChangeRequest_ValidJustification_Succeeds()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new ProjectService(db, CreateMockAiService());

        var project = await service.CreateProject(_userId, new CreateProjectRequest("App", "Desc", null));
        var justification = "This is a well-thought-out justification that exceeds fifty characters in length for the project change.";

        var result = await service.SubmitChangeRequest(_userId, new ProjectChangeRequestDto(
            "New App", "New desc", justification, project.Id));

        Assert.Equal("Pending", result.Status);
        Assert.Equal("New App", result.ProposedProjectTitle);
    }

    [Fact]
    public async Task ApproveChangeRequest_DropsOldAndCreatesNew()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new ProjectService(db, CreateMockAiService());

        var oldProject = await service.CreateProject(_userId, new CreateProjectRequest("Old App", "Old", null));
        var justification = "This is a well-thought-out justification that exceeds fifty characters in length for the project change.";

        var changeReq = await service.SubmitChangeRequest(_userId, new ProjectChangeRequestDto(
            "New App", "New desc", justification, oldProject.Id));

        var newProject = await service.ApproveChangeRequest(_userId, changeReq.Id);

        Assert.Equal("New App", newProject.Title);
        Assert.Equal("Active", newProject.Status);

        // Old project should be dropped
        var allProjects = await service.GetAllProjects(_userId);
        var old = allProjects.First(p => p.Id == oldProject.Id);
        Assert.Equal("Dropped", old.Status);
    }

    [Fact]
    public async Task GetActiveProjects_FiltersCorrectly()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var service = new ProjectService(db, CreateMockAiService());

        var p1 = await service.CreateProject(_userId, new CreateProjectRequest("Active 1", "Desc", null));
        await service.CreateProject(_userId, new CreateProjectRequest("Active 2", "Desc", null));
        await service.UpdateStatus(_userId, p1.Id, new UpdateProjectStatusRequest("Completed"));

        var active = await service.GetActiveProjects(_userId);
        Assert.Single(active);
        Assert.Equal("Active 2", active[0].Title);
    }
}
