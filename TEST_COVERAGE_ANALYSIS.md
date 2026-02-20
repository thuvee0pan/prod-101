# Test Coverage Analysis — ExecutionOS

## Current State

**Test coverage: 0%.** The codebase has no test files, no test projects, no testing frameworks installed, and no test scripts configured on either the backend (.NET 8 / C#) or frontend (Next.js / TypeScript).

### Inventory

| Layer | Source Files | Test Files | Coverage |
|-------|-------------|------------|----------|
| Backend Services | 7 | 0 | 0% |
| Backend Controllers | 6 | 0 | 0% |
| Backend Models/DTOs | 13 | 0 | 0% |
| Backend Jobs | 1 | 0 | 0% |
| Backend Data | 1 | 0 | 0% |
| Frontend Pages | 6 | 0 | 0% |
| Frontend Components | 4 | 0 | 0% |
| Frontend Lib/Types | 2 | 0 | 0% |

---

## Proposed Test Improvements (Prioritized)

### Priority 1 — Backend Service Unit Tests (Highest Risk)

The service layer contains all business logic and domain rules. Bugs here directly corrupt user data. These are also the easiest to unit test by using EF Core's in-memory provider or SQLite.

#### 1.1 `StreakService` — **Critical**

File: `backend/ExecutionOS.API/Services/StreakService.cs`

This service has the most fragile logic in the codebase: date-based streak continuation/reset calculations. The `UpdateStreak` method (line 52-88) determines whether a streak continues or resets based on whether `LastLoggedDate == yesterday`. Edge cases that need test coverage:

- **Streak continuation**: logging on consecutive days increments `CurrentCount`
- **Streak break**: missing a day resets `CurrentCount` to 0
- **Streak restart**: logging after a gap starts a fresh streak at 1
- **Longest streak tracking**: `LongestCount` updates only when `CurrentCount` surpasses it
- **First-time logging**: creating a streak from scratch when no record exists
- **Same-day re-log**: logging twice on the same day should not double-increment
- **Non-achievement**: when `achieved` is false, streak resets regardless of date continuity

#### 1.2 `GoalService` — **Critical**

File: `backend/ExecutionOS.API/Services/GoalService.cs`

Enforces the "one active goal at a time" constraint (line 17-21). Test cases:

- **Constraint enforcement**: creating a second active goal throws `InvalidOperationException`
- **Goal lifecycle**: creating, completing, and abandoning goals transitions status correctly
- **Goal not found**: completing/abandoning a non-existent or already-completed goal throws
- **Days remaining calculation**: `MapToResponse` (line 83-100) calculates `daysRemaining` and `daysElapsed` — needs verification at boundary conditions (day 0, day 90, past end date)

#### 1.3 `ProjectService` — **Critical**

File: `backend/ExecutionOS.API/Services/ProjectService.cs`

Enforces the 2-project cap and handles the AI-gated project change workflow. Test cases:

- **Max active projects constraint**: creating a 3rd active project throws (line 25-28)
- **Justification length validation**: justification under 50 characters is rejected (line 82-84)
- **Change request approval flow**: approving drops the old project, creates the new one (line 119-151)
- **Status update validation**: invalid status string throws `ArgumentException` (line 70-71)
- **AI service integration**: mock `AiService` to verify `EvaluateProjectChange` is called with correct arguments

#### 1.4 `DailyLogService` — **High**

File: `backend/ExecutionOS.API/Services/DailyLogService.cs`

Handles the upsert logic for daily logs. Test cases:

- **Create new log**: first log for a day creates a new record
- **Update existing log**: second log for same day updates rather than duplicates
- **Streak integration**: verify `StreakService.UpdateStreaks` is called after save
- **Date range queries**: `GetLogs` correctly filters by `from`/`to` dates

#### 1.5 `WeeklyReviewService` — **High**

File: `backend/ExecutionOS.API/Services/WeeklyReviewService.cs`

Orchestrates data gathering and AI review generation. Test cases:

- **Week boundary calculation**: verify `weekStart` is Monday and `weekEnd` is Sunday (line 22-23) — this is error-prone and depends on `DayOfWeek` arithmetic
- **Empty week handling**: no logs in the week produces "No daily logs recorded" message
- **Data aggregation**: all streaks, goals, projects are gathered correctly for the AI prompt
- **Review persistence**: generated review is saved to DB with correct fields
- **AI service mock**: verify the AI service receives a properly constructed `WeeklyReviewContext`

### Priority 2 — AI Service Tests (Parse Logic + Error Handling)

#### 2.1 `AiService.ParseWeeklyReview` — **High**

File: `backend/ExecutionOS.API/Services/AiService.cs`, lines 103-136

This is a pure function that parses free-form AI text into structured sections. It's fragile because it relies on string matching (`"what worked"`, `"avoided"`, `"cut"`). Test cases:

- **Standard format**: AI response with clear section headers parses correctly
- **Variant headers**: "WHERE I AVOIDED" vs "WHAT I AVOIDED" both match (line 115)
- **Empty sections**: missing sections produce empty strings, not nulls or crashes
- **No sections detected**: completely unstructured response handled gracefully
- **False positive on "cut"**: line 116 matches any line containing "cut" — test that a line like "I cut deep work short" doesn't accidentally trigger section 3

#### 2.2 `AiService.CallAi` — **Medium**

File: `backend/ExecutionOS.API/Services/AiService.cs`, lines 66-101

- **Missing API key**: returns fallback message (line 72) — verify this path
- **HTTP error handling**: currently no handling for non-200 responses or `JsonDocument.Parse` failures — tests should document this gap
- **`HttpClient` disposal**: new `HttpClient` per call (line 74) — tests should highlight this as a performance concern

### Priority 3 — Background Job Tests

#### 3.1 `InactivityDetectionJob` — **High**

File: `backend/ExecutionOS.API/Jobs/InactivityDetectionJob.cs`

This runs unsupervised every 24 hours and creates warnings for users. Bugs here go unnoticed. Test cases:

- **No logs ever**: user with zero daily logs gets a "never logged" warning
- **Stale logs**: user whose last log is >7 days ago gets inactivity warning
- **Active user**: user who logged within 7 days gets no warning
- **Duplicate suppression**: existing recent warning prevents creating another (line 63-65)
- **Stale project detection**: active project with no mentions in notes within 7 days triggers warning
- **Project mention matching**: `Notes.Contains(project.Title)` (line 90) — test case sensitivity and partial matches

### Priority 4 — Controller / API Integration Tests

#### 4.1 `DashboardController.CalculateOverallScore` — **Medium**

File: `backend/ExecutionOS.API/Controllers/DashboardController.cs`, lines 66-80

Pure calculation logic embedded in a controller. Test cases:

- **No logs**: returns 0
- **Perfect week**: all 7 days meeting all targets returns 100
- **Partial week**: 3/7 days logged calculates against 7-day denominator (not 3)
- **Threshold values**: deep work at exactly 120 min counts, 119 does not; learning at exactly 30 min counts, 29 does not

#### 4.2 `GetUserId` pattern — **Medium**

All controllers (lines like `GoalsController.cs:16`) extract user ID from `X-User-Id` header with a fallback to `Guid.Empty`. Test cases:

- **Missing header**: falls back to `Guid.Empty` — verify services handle this gracefully
- **Invalid GUID**: `Guid.Parse` will throw `FormatException` — no catch exists
- **Empty header**: behavior when header is present but empty string

### Priority 5 — Frontend Tests

#### 5.1 `api.ts` Client — **Medium**

File: `frontend/src/lib/api.ts`

- **Error handling**: non-OK responses throw with `body.error` message (line 29)
- **204 response**: returns `undefined as T` (line 32) — verify callers handle this
- **Network failure**: no retry or timeout logic exists — tests document this gap

#### 5.2 Component Tests — **Low-Medium**

Components like `ScoreRing.tsx`, `StreakBar.tsx`, and `WarningBanner.tsx` are data-display components. Basic render tests with React Testing Library would verify:

- Components render without crashing for valid props
- Edge cases: zero scores, empty streaks, no warnings
- Conditional rendering (e.g., warning banner only shows when warnings exist)

---

## Recommended Test Infrastructure Setup

### Backend

1. Create `ExecutionOS.Tests` project using xUnit
2. Add `Microsoft.EntityFrameworkCore.InMemoryDatabase` for service tests
3. Add `Moq` or `NSubstitute` for mocking `AiService` and `HttpClient`
4. Add `Microsoft.AspNetCore.Mvc.Testing` for integration tests
5. Register in `ExecutionOS.sln`

```bash
dotnet new xunit -n ExecutionOS.Tests -o backend/ExecutionOS.Tests
dotnet add backend/ExecutionOS.Tests reference backend/ExecutionOS.API
dotnet add backend/ExecutionOS.Tests package Microsoft.EntityFrameworkCore.InMemory
dotnet add backend/ExecutionOS.Tests package Moq
dotnet add backend/ExecutionOS.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet sln backend/ExecutionOS.sln add backend/ExecutionOS.Tests
```

### Frontend

1. Install Vitest (lighter than Jest for Next.js projects)
2. Add React Testing Library for component tests
3. Add `msw` (Mock Service Worker) for API client tests

```bash
npm install -D vitest @testing-library/react @testing-library/jest-dom jsdom msw
```

---

## Summary of Top Bugs Likely Lurking Without Tests

| # | Location | Risk |
|---|----------|------|
| 1 | `StreakService.UpdateStreak` — same-day re-logging may double-increment streaks | **High** |
| 2 | `AiService.ParseWeeklyReview` — "cut" keyword false positive on section 3 detection | **High** |
| 3 | `DashboardController.CalculateOverallScore` — `maxDays` always 7 even with 0 logs, but early return covers it; partial weeks may give misleadingly low scores | **Medium** |
| 4 | `WeeklyReviewService.Generate` — week start calculation may be wrong when `DayOfWeek` is Sunday (AddDays returns Tuesday of previous week) | **High** |
| 5 | All controllers — `Guid.Parse` on missing/malformed `X-User-Id` header crashes with unhandled `FormatException` | **Medium** |
| 6 | `InactivityDetectionJob` — `Notes.Contains(project.Title)` is case-sensitive, may miss mentions | **Low** |
| 7 | `AiService.CallAi` — no error handling for failed HTTP responses or malformed JSON from OpenAI | **High** |
