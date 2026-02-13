# Personal Execution OS - System Architecture

## Overview

A discipline-driven execution tracker that enforces focus, tracks daily habits, detects stagnation, and delivers weekly AI-powered accountability reviews.

## Core Constraints (By Design)

- **ONE** primary 90-day goal at a time
- **TWO** active projects maximum
- Must justify adding new projects (anti-idea-hopping)
- 7-day inactivity triggers warning system

## Tech Stack

| Layer      | Technology             |
|------------|------------------------|
| Frontend   | Next.js 14 (App Router, TypeScript, Tailwind CSS) |
| Backend    | .NET 8 Web API (C#)   |
| Database   | PostgreSQL 16          |
| AI         | OpenAI GPT-4 API      |
| Hosting    | Docker Compose (dev)   |

## Database Schema

```
┌─────────────────────┐     ┌─────────────────────────┐
│ users               │     │ goals                   │
│─────────────────────│     │─────────────────────────│
│ id (PK)             │──┐  │ id (PK)                 │
│ email               │  │  │ user_id (FK)            │
│ name                │  │  │ title                   │
│ created_at          │  │  │ description             │
│ updated_at          │  │  │ start_date              │
│                     │  │  │ end_date (90 days)      │
└─────────────────────┘  │  │ status (active/done/    │
                         │  │         abandoned)       │
                         │  │ created_at              │
                         └──│ updated_at              │
                            └─────────────────────────┘
                                       │
┌─────────────────────────┐            │
│ projects                │            │
│─────────────────────────│            │
│ id (PK)                 │            │
│ user_id (FK)            │            │
│ goal_id (FK) ───────────│────────────┘
│ title                   │
│ description             │
│ status (active/paused/  │
│         completed/      │
│         dropped)        │
│ justification           │  ← required when adding while 2 active exist
│ created_at              │
│ updated_at              │
└─────────────────────────┘

┌─────────────────────────┐     ┌─────────────────────────┐
│ daily_logs              │     │ streaks                 │
│─────────────────────────│     │─────────────────────────│
│ id (PK)                 │     │ id (PK)                 │
│ user_id (FK)            │     │ user_id (FK)            │
│ log_date                │     │ streak_type (deepwork/  │
│ deep_work_minutes       │     │   gym/learning/sober)   │
│ gym_completed (bool)    │     │ current_count           │
│ learning_minutes        │     │ longest_count           │
│ alcohol_free (bool)     │     │ last_logged_date        │
│ notes                   │     │ updated_at              │
│ created_at              │     └─────────────────────────┘
│ updated_at              │
└─────────────────────────┘

┌─────────────────────────┐     ┌─────────────────────────┐
│ inactivity_warnings     │     │ weekly_reviews          │
│─────────────────────────│     │─────────────────────────│
│ id (PK)                 │     │ id (PK)                 │
│ user_id (FK)            │     │ user_id (FK)            │
│ warning_type            │     │ week_start              │
│ message                 │     │ week_end                │
│ triggered_at            │     │ what_worked             │
│ acknowledged (bool)     │     │ where_avoided           │
│ acknowledged_at         │     │ what_to_cut             │
└─────────────────────────┘     │ ai_summary              │
                                │ generated_at            │
┌─────────────────────────┐     └─────────────────────────┘
│ project_change_requests │
│─────────────────────────│
│ id (PK)                 │
│ user_id (FK)            │
│ proposed_project_title  │
│ justification           │
│ replace_project_id (FK) │  ← which project to drop
│ status (pending/        │
│         approved/denied)│
│ reviewed_at             │
│ created_at              │
└─────────────────────────┘
```

## API Structure

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`

### Goals
- `POST /api/goals` - Create 90-day goal (fails if active goal exists)
- `GET /api/goals/active` - Get current active goal
- `GET /api/goals` - List all goals
- `PUT /api/goals/{id}/complete` - Mark goal complete
- `PUT /api/goals/{id}/abandon` - Abandon goal (requires reason)

### Projects
- `POST /api/projects` - Add project (fails if 2 active, unless justification provided)
- `GET /api/projects/active` - Get active projects (max 2)
- `GET /api/projects` - List all projects
- `PUT /api/projects/{id}/status` - Update project status
- `POST /api/projects/change-request` - Request to swap a project (anti-idea-hopping)

### Daily Logs
- `POST /api/daily-logs` - Log today's execution
- `GET /api/daily-logs?from=&to=` - Get logs for date range
- `GET /api/daily-logs/today` - Get today's log
- `GET /api/daily-logs/streaks` - Get all streak data

### Warnings
- `GET /api/warnings` - Get active warnings
- `PUT /api/warnings/{id}/acknowledge` - Acknowledge a warning

### Weekly Reviews
- `POST /api/weekly-reviews/generate` - Trigger AI review for current week
- `GET /api/weekly-reviews` - List past reviews
- `GET /api/weekly-reviews/latest` - Get most recent review

### Dashboard
- `GET /api/dashboard` - Aggregated view (goal, projects, streaks, warnings)

## AI Logic Flow

### Weekly Review Generation
```
1. Collect last 7 days of daily_logs
2. Collect active projects + their progress
3. Collect current goal status
4. Build prompt:
   - "Here is my execution data for the week: {data}"
   - "Analyze: What worked? Where did I avoid hard work? What should I cut?"
   - "Be direct. No motivational fluff. Call out patterns."
5. Parse AI response into structured sections
6. Store in weekly_reviews table
```

### Inactivity Detection (Background Job)
```
Every 24 hours:
1. For each user, check last daily_log date
2. If gap >= 7 days → create inactivity_warning
3. Check each active project for progress signals
4. If project has no associated daily_log notes mentioning it for 7+ days → warn
```

### Project Change Gatekeeper
```
When user tries to add project #3:
1. Block the addition
2. Require: justification text (min 50 chars)
3. Require: which existing project to drop or pause
4. AI evaluates justification quality (is this idea-hopping or legitimate pivot?)
5. Return recommendation (approve/deny with reasoning)
6. User makes final call
```
