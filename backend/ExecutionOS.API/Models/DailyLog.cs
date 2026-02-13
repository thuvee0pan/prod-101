namespace ExecutionOS.API.Models;

public class DailyLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public DateOnly LogDate { get; set; }
    public int DeepWorkMinutes { get; set; }
    public bool GymCompleted { get; set; }
    public int LearningMinutes { get; set; }
    public bool AlcoholFree { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
