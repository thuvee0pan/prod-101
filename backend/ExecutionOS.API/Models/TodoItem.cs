namespace ExecutionOS.API.Models;

public enum TodoCategory
{
    Work,
    Personal,
    Gym,
    Learning,
    Health,
    Finance,
    Social,
    Other
}

public enum TodoStatus
{
    Pending,
    InProgress,
    Done
}

public class TodoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TodoCategory Category { get; set; } = TodoCategory.Personal;
    public TodoStatus Status { get; set; } = TodoStatus.Pending;
    public DateOnly DueDate { get; set; }
    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
