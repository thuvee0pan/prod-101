namespace ExecutionOS.API.DTOs;

public record DashboardResponse(
    GoalResponse? ActiveGoal,
    List<ProjectResponse> ActiveProjects,
    List<StreakResponse> Streaks,
    List<WarningResponse> ActiveWarnings,
    WeeklyReviewResponse? LatestReview,
    DailyLogResponse? TodayLog,
    ExecutionScore Score
);

public record WarningResponse(
    Guid Id,
    string WarningType,
    string Message,
    DateTime TriggeredAt,
    bool Acknowledged
);

public record ExecutionScore(
    int WeeklyDeepWorkMinutes,
    int WeeklyGymDays,
    int WeeklyLearningMinutes,
    int WeeklySoberDays,
    double OverallPercentage
);
