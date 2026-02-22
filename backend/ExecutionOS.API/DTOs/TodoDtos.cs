namespace ExecutionOS.API.DTOs;

public record CreateTodoRequest(
    string Title,
    string? Description,
    string Category,
    string? DueDate
);

public record UpdateTodoRequest(
    string? Title,
    string? Description,
    string? Category,
    string? Status,
    string? DueDate
);

public record TodoResponse(
    Guid Id,
    string Title,
    string? Description,
    string Category,
    string Status,
    string DueDate,
    int SortOrder,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
