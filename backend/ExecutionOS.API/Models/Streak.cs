namespace ExecutionOS.API.Models;

public enum StreakType
{
    DeepWork,
    Gym,
    Learning,
    Sober
}

public class Streak
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public StreakType StreakType { get; set; }
    public int CurrentCount { get; set; }
    public int LongestCount { get; set; }
    public DateOnly LastLoggedDate { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
