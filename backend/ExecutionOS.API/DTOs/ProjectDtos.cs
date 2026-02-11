namespace ExecutionOS.API.DTOs;

public record CreateProjectRequest(
    string Title,
    string Description,
    Guid? GoalId
);

public record ProjectResponse(
    Guid Id,
    string Title,
    string Description,
    string Status,
    Guid? GoalId,
    DateTime CreatedAt
);

public record UpdateProjectStatusRequest(string Status);

public record ProjectChangeRequestDto(
    string ProposedProjectTitle,
    string ProposedProjectDescription,
    string Justification,
    Guid ReplaceProjectId
);

public record ProjectChangeResponse(
    Guid Id,
    string ProposedProjectTitle,
    string Justification,
    string Status,
    string? AiRecommendation,
    DateTime CreatedAt
);
