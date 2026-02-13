namespace ExecutionOS.API.DTOs;

public record CreateGoalRequest(string Title, string Description);

public record GoalResponse(
    Guid Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    int DaysRemaining,
    int DaysElapsed,
    DateTime CreatedAt
);

public record AbandonGoalRequest(string Reason);
