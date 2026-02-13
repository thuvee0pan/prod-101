namespace ExecutionOS.API.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<Goal> Goals { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
    public List<DailyLog> DailyLogs { get; set; } = new();
    public List<Streak> Streaks { get; set; } = new();
    public List<InactivityWarning> Warnings { get; set; } = new();
    public List<WeeklyReview> WeeklyReviews { get; set; } = new();
}
