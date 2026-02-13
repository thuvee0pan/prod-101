using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ExecutionOS.API.Services;

public class AiService
{
    private readonly IConfiguration _config;

    public AiService(IConfiguration config) => _config = config;

    public async Task<WeeklyReviewAiResult> GenerateWeeklyReview(WeeklyReviewContext context)
    {
        var prompt = $"""
            You are a strict accountability coach. No motivational fluff. Be direct and honest.

            Here is my execution data for the past week:

            **Goal:** {context.GoalTitle} (Day {context.GoalDayNumber}/90)

            **Active Projects:** {string.Join(", ", context.ActiveProjects)}

            **Daily Execution (last 7 days):**
            {context.DailyLogsSummary}

            **Streaks:**
            - Deep Work: {context.DeepWorkStreak} days
            - Gym: {context.GymStreak} days
            - Learning: {context.LearningStreak} days
            - Sober: {context.SoberStreak} days

            Analyze three things:
            1. WHAT WORKED - Be specific about which days/habits showed real execution
            2. WHERE I AVOIDED HARD WORK - Call out patterns of avoidance, low-effort days, skipped sessions
            3. WHAT TO CUT - Identify distractions, habits, or projects that should be eliminated

            Keep each section to 2-3 sentences. No encouragement. Just truth.
            """;

        var response = await CallAi(prompt);

        return ParseWeeklyReview(response);
    }

    public async Task<string> EvaluateProjectChange(string newProject, string justification, string replacingProject)
    {
        var prompt = $"""
            You are an anti-idea-hopping gatekeeper. A user wants to switch projects.

            **Current project to DROP:** {replacingProject}
            **Proposed new project:** {newProject}
            **Their justification:** {justification}

            Evaluate:
            1. Is this a legitimate strategic pivot or are they bored and chasing novelty?
            2. Does the justification show clear reasoning or vague excitement?

            Respond with APPROVE or DENY followed by a 2-sentence explanation.
            Be harsh. Most project switches are procrastination disguised as productivity.
            """;

        return await CallAi(prompt);
    }

    private async Task<string> CallAi(string prompt)
    {
        var apiKey = _config["AiSettings:ApiKey"];
        var model = _config["AiSettings:Model"] ?? "gpt-4";

        if (string.IsNullOrEmpty(apiKey))
            return "[AI not configured - set AiSettings:ApiKey in appsettings.json]";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var requestBody = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = "You are a strict accountability coach. No fluff. Direct and honest." },
                new { role = "user", content = prompt }
            },
            max_tokens = 500,
            temperature = 0.7
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseBody);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }

    private static WeeklyReviewAiResult ParseWeeklyReview(string response)
    {
        var sections = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var whatWorked = new List<string>();
        var whereAvoided = new List<string>();
        var whatToCut = new List<string>();

        var currentSection = 0;
        foreach (var line in sections)
        {
            var lower = line.ToLower();
            if (lower.Contains("what worked")) { currentSection = 1; continue; }
            if (lower.Contains("avoided") || lower.Contains("where i")) { currentSection = 2; continue; }
            if (lower.Contains("what to cut") || lower.Contains("cut")) { currentSection = 3; continue; }

            var cleaned = line.TrimStart('-', '*', ' ', '\t');
            if (string.IsNullOrWhiteSpace(cleaned)) continue;

            switch (currentSection)
            {
                case 1: whatWorked.Add(cleaned); break;
                case 2: whereAvoided.Add(cleaned); break;
                case 3: whatToCut.Add(cleaned); break;
            }
        }

        return new WeeklyReviewAiResult
        {
            WhatWorked = string.Join(" ", whatWorked),
            WhereAvoided = string.Join(" ", whereAvoided),
            WhatToCut = string.Join(" ", whatToCut),
            FullResponse = response
        };
    }
}

public class WeeklyReviewContext
{
    public string GoalTitle { get; set; } = string.Empty;
    public int GoalDayNumber { get; set; }
    public List<string> ActiveProjects { get; set; } = new();
    public string DailyLogsSummary { get; set; } = string.Empty;
    public int DeepWorkStreak { get; set; }
    public int GymStreak { get; set; }
    public int LearningStreak { get; set; }
    public int SoberStreak { get; set; }
}

public class WeeklyReviewAiResult
{
    public string WhatWorked { get; set; } = string.Empty;
    public string WhereAvoided { get; set; } = string.Empty;
    public string WhatToCut { get; set; } = string.Empty;
    public string FullResponse { get; set; } = string.Empty;
}
