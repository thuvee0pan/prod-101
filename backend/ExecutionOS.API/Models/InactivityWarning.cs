namespace ExecutionOS.API.Models;

public class InactivityWarning
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string WarningType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public bool Acknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }

    public User User { get; set; } = null!;
}
