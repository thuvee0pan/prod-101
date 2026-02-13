namespace ExecutionOS.API.Models;

public enum ChangeRequestStatus
{
    Pending,
    Approved,
    Denied
}

public class ProjectChangeRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string ProposedProjectTitle { get; set; } = string.Empty;
    public string ProposedProjectDescription { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
    public Guid? ReplaceProjectId { get; set; }
    public ChangeRequestStatus Status { get; set; } = ChangeRequestStatus.Pending;
    public string? AiRecommendation { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Project? ReplaceProject { get; set; }
}
