namespace ExecutionOS.API.DTOs;

public record WeeklyReviewResponse(
    Guid Id,
    DateOnly WeekStart,
    DateOnly WeekEnd,
    string WhatWorked,
    string WhereAvoided,
    string WhatToCut,
    string AiSummary,
    DateTime GeneratedAt
);
