using ExecutionOS.API.Services;

namespace ExecutionOS.Tests.Services;

public class AiServiceTests
{
    [Fact]
    public void ParseWeeklyReview_StandardFormat_ParsesCorrectly()
    {
        var response = """
            ## WHAT WORKED
            - Monday and Tuesday had solid 3-hour deep work sessions.
            - Gym was consistent all 5 weekdays.

            ## WHERE I AVOIDED HARD WORK
            - Thursday and Friday had under 30 minutes of deep work.
            - No learning logged on Wednesday.

            ## WHAT TO CUT
            - Social media browsing between sessions.
            - The side project that hasn't moved in 2 weeks.
            """;

        var result = AiService.ParseWeeklyReview(response);

        Assert.Contains("Monday and Tuesday", result.WhatWorked);
        Assert.Contains("Gym was consistent", result.WhatWorked);
        Assert.Contains("Thursday and Friday", result.WhereAvoided);
        Assert.Contains("Social media", result.WhatToCut);
        Assert.Equal(response, result.FullResponse);
    }

    [Fact]
    public void ParseWeeklyReview_NumberedHeaders_ParsesCorrectly()
    {
        var response = """
            1. WHAT WORKED
            Gym attendance was perfect this week.

            2. WHERE I AVOIDED HARD WORK
            Deep work sessions were cut short on 3 days.

            3. WHAT TO CUT
            Evening Netflix sessions killing next-day productivity.
            """;

        var result = AiService.ParseWeeklyReview(response);

        Assert.Contains("Gym attendance", result.WhatWorked);
        Assert.Contains("cut short", result.WhereAvoided);
        Assert.Contains("Netflix", result.WhatToCut);
    }

    [Fact]
    public void ParseWeeklyReview_NoSections_ReturnsEmptyStrings()
    {
        var response = "This is just a random response with no sections at all.";

        var result = AiService.ParseWeeklyReview(response);

        Assert.Empty(result.WhatWorked);
        Assert.Empty(result.WhereAvoided);
        Assert.Empty(result.WhatToCut);
        Assert.Equal(response, result.FullResponse);
    }

    [Fact]
    public void ParseWeeklyReview_CutInContent_DoesNotFalsePositive()
    {
        // The word "cut" in content should NOT trigger section 3
        var response = """
            ## WHAT WORKED
            - I cut my social media time in half this week.
            - Deep work was solid on Monday.

            ## WHERE I AVOIDED HARD WORK
            - I cut corners on the learning sessions.

            ## WHAT TO CUT
            - Remove the daily standup that adds no value.
            """;

        var result = AiService.ParseWeeklyReview(response);

        // "I cut my social media time" should be in WhatWorked, not WhatToCut
        Assert.Contains("cut my social media", result.WhatWorked);
        Assert.Contains("Deep work was solid", result.WhatWorked);
        Assert.Contains("cut corners", result.WhereAvoided);
        Assert.Contains("daily standup", result.WhatToCut);
    }

    [Fact]
    public void ParseWeeklyReview_BoldHeaders_ParsesCorrectly()
    {
        var response = """
            **WHAT WORKED**
            - Great consistency with gym.

            **WHERE YOU AVOIDED HARD WORK**
            - Skipped deep work on Friday.

            **WHAT TO CUT**
            - Drop the blog project.
            """;

        var result = AiService.ParseWeeklyReview(response);

        Assert.Contains("consistency with gym", result.WhatWorked);
        Assert.Contains("Skipped deep work", result.WhereAvoided);
        Assert.Contains("blog project", result.WhatToCut);
    }

    [Fact]
    public void ParseWeeklyReview_EmptyResponse_HandledGracefully()
    {
        var result = AiService.ParseWeeklyReview("");

        Assert.Empty(result.WhatWorked);
        Assert.Empty(result.WhereAvoided);
        Assert.Empty(result.WhatToCut);
        Assert.Empty(result.FullResponse);
    }

    [Fact]
    public void ParseWeeklyReview_MissingSections_PartialParsing()
    {
        var response = """
            ## WHAT WORKED
            - Nailed the gym routine.
            - Learning was strong.
            """;

        var result = AiService.ParseWeeklyReview(response);

        Assert.Contains("gym routine", result.WhatWorked);
        Assert.Contains("Learning was strong", result.WhatWorked);
        Assert.Empty(result.WhereAvoided);
        Assert.Empty(result.WhatToCut);
    }
}
