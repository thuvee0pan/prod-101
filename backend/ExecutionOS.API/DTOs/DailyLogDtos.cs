namespace ExecutionOS.API.DTOs;

public record CreateDailyLogRequest(
    int DeepWorkMinutes,
    bool GymCompleted,
    int LearningMinutes,
    bool AlcoholFree,
    string? Notes
);

public record DailyLogResponse(
    Guid Id,
    DateOnly LogDate,
    int DeepWorkMinutes,
    bool GymCompleted,
    int LearningMinutes,
    bool AlcoholFree,
    string? Notes,
    DateTime CreatedAt
);

public record StreakResponse(
    string StreakType,
    int CurrentCount,
    int LongestCount,
    DateOnly LastLoggedDate
);
