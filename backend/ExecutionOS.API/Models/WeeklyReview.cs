namespace ExecutionOS.API.Models;

public class WeeklyReview
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }
    public string WhatWorked { get; set; } = string.Empty;
    public string WhereAvoided { get; set; } = string.Empty;
    public string WhatToCut { get; set; } = string.Empty;
    public string AiSummary { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
