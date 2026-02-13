using ExecutionOS.API.Data;
using ExecutionOS.API.DTOs;
using ExecutionOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExecutionOS.API.Services;

public class ProjectService
{
    private readonly AppDbContext _db;
    private readonly AiService _ai;
    private const int MaxActiveProjects = 2;

    public ProjectService(AppDbContext db, AiService ai)
    {
        _db = db;
        _ai = ai;
    }

    public async Task<ProjectResponse> CreateProject(Guid userId, CreateProjectRequest request)
    {
        var activeCount = await _db.Projects
            .CountAsync(p => p.UserId == userId && p.Status == ProjectStatus.Active);

        if (activeCount >= MaxActiveProjects)
            throw new InvalidOperationException(
                $"You already have {MaxActiveProjects} active projects. " +
                "Submit a project change request with justification to add a new one.");

        var project = new Project
        {
            UserId = userId,
            GoalId = request.GoalId,
            Title = request.Title,
            Description = request.Description
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        return MapToResponse(project);
    }

    public async Task<List<ProjectResponse>> GetActiveProjects(Guid userId)
    {
        var projects = await _db.Projects
            .Where(p => p.UserId == userId && p.Status == ProjectStatus.Active)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();

        return projects.Select(MapToResponse).ToList();
    }

    public async Task<List<ProjectResponse>> GetAllProjects(Guid userId)
    {
        var projects = await _db.Projects
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return projects.Select(MapToResponse).ToList();
    }

    public async Task<ProjectResponse> UpdateStatus(Guid userId, Guid projectId, UpdateProjectStatusRequest request)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId)
            ?? throw new InvalidOperationException("Project not found.");

        if (!Enum.TryParse<ProjectStatus>(request.Status, true, out var status))
            throw new ArgumentException($"Invalid status: {request.Status}");

        project.Status = status;
        project.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return MapToResponse(project);
    }

    public async Task<ProjectChangeResponse> SubmitChangeRequest(Guid userId, ProjectChangeRequestDto request)
    {
        if (request.Justification.Length < 50)
            throw new InvalidOperationException(
                "Justification must be at least 50 characters. Explain why this switch is necessary, not just exciting.");

        var replaceProject = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == request.ReplaceProjectId && p.UserId == userId && p.Status == ProjectStatus.Active)
            ?? throw new InvalidOperationException("Project to replace not found or not active.");

        var aiRecommendation = await _ai.EvaluateProjectChange(
            request.ProposedProjectTitle,
            request.Justification,
            replaceProject.Title
        );

        var changeRequest = new ProjectChangeRequest
        {
            UserId = userId,
            ProposedProjectTitle = request.ProposedProjectTitle,
            ProposedProjectDescription = request.ProposedProjectDescription,
            Justification = request.Justification,
            ReplaceProjectId = request.ReplaceProjectId,
            AiRecommendation = aiRecommendation
        };

        _db.ProjectChangeRequests.Add(changeRequest);
        await _db.SaveChangesAsync();

        return new ProjectChangeResponse(
            changeRequest.Id,
            changeRequest.ProposedProjectTitle,
            changeRequest.Justification,
            changeRequest.Status.ToString(),
            changeRequest.AiRecommendation,
            changeRequest.CreatedAt
        );
    }

    public async Task<ProjectResponse> ApproveChangeRequest(Guid userId, Guid requestId)
    {
        var changeRequest = await _db.ProjectChangeRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId && r.Status == ChangeRequestStatus.Pending)
            ?? throw new InvalidOperationException("Change request not found or already processed.");

        // Drop the old project
        if (changeRequest.ReplaceProjectId.HasValue)
        {
            var oldProject = await _db.Projects.FindAsync(changeRequest.ReplaceProjectId.Value);
            if (oldProject != null)
            {
                oldProject.Status = ProjectStatus.Dropped;
                oldProject.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Create the new project
        var newProject = new Project
        {
            UserId = userId,
            Title = changeRequest.ProposedProjectTitle,
            Description = changeRequest.ProposedProjectDescription
        };

        changeRequest.Status = ChangeRequestStatus.Approved;
        changeRequest.ReviewedAt = DateTime.UtcNow;

        _db.Projects.Add(newProject);
        await _db.SaveChangesAsync();

        return MapToResponse(newProject);
    }

    private static ProjectResponse MapToResponse(Project project) =>
        new(project.Id, project.Title, project.Description, project.Status.ToString(), project.GoalId, project.CreatedAt);
}
