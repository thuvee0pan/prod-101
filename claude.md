# Execution OS - Project Rules

## Overview
Personal Execution OS — a full-stack accountability app. One 90-day goal, max 2 active projects, daily logging, streak tracking, AI-powered weekly reviews, and daily todos with categories.

## Tech Stack
- **Frontend**: Next.js 14 (App Router), React 18, TypeScript, Tailwind CSS 3.4
- **Backend**: .NET 8 Web API, C#, Entity Framework Core 8
- **Database**: PostgreSQL 16
- **AI**: OpenAI GPT-4 (weekly reviews, project change evaluation)
- **Deployment**: Docker Compose

## Project Structure
```
backend/ExecutionOS.API/
  Controllers/    # API controllers (REST, [ApiController] attribute)
  Data/           # AppDbContext (EF Core)
  DTOs/           # Request/Response records
  Jobs/           # Background hosted services
  Migrations/     # Raw SQL migration files
  Models/         # EF Core entity classes
  Services/       # Business logic layer
  Program.cs      # DI registration, middleware pipeline

frontend/src/
  app/            # Next.js App Router pages (each folder = route)
  components/     # Shared React components
  lib/api.ts      # API client functions (fetch wrapper)
  types/api.ts    # TypeScript interfaces for API responses
```

## Conventions

### Backend
- **Models**: Classes in `Models/` with `Guid Id`, `Guid UserId`, `DateTime CreatedAt/UpdatedAt`. Enums defined in same file as the model.
- **DTOs**: Records in `DTOs/` — separate `Create*Request`, `Update*Request`, and `*Response` records. One file per domain (e.g., `GoalDtos.cs`).
- **Services**: One service per domain in `Services/`. Inject `AppDbContext` via constructor. Async/await. Throw `InvalidOperationException` for business rule violations. Private `MapToResponse()` method for entity-to-DTO mapping.
- **Controllers**: `[ApiController]` + `[Route("api/[controller]")]`. One controller per domain. `GetUserId()` helper reads `X-User-Id` header (MVP auth placeholder). Returns `ActionResult<T>`.
- **DI**: Register services as `AddScoped<T>()` in `Program.cs`.
- **DB**: Add `DbSet<T>` to `AppDbContext`. Configure relationships in `OnModelCreating` with Fluent API. Enum properties use `.HasConversion<string>()`.
- **Migrations**: Sequential numbered SQL files in `Migrations/` (e.g., `001_InitialSchema.sql`, `002_AddTodos.sql`). Also mounted in `docker-compose.yml`.

### Frontend
- **Pages**: `'use client'` components in `app/<route>/page.tsx`. Use `useState` + `useEffect` for data fetching. `load()` function pattern for refresh.
- **API calls**: All go through `lib/api.ts` which wraps `fetch()` with JSON headers and `X-User-Id`. Generic `request<T>()` function.
- **Types**: Interfaces in `types/api.ts` matching backend response DTOs (camelCase).
- **Styling**: Tailwind utility classes. Custom color tokens: `bg`, `surface`, `border`, `accent`, `accent-dim`, `warning`, `danger`, `muted`. Component classes in `globals.css`: `.card`, `.btn-primary`, `.btn-danger`, `.btn-secondary`, `.input`, `.label`.
- **Navigation**: Sidebar component with `nav` array of `{ href, label }` objects.

## Key Business Rules
- One active 90-day goal per user at a time
- Maximum 2 active projects per user
- Project changes require AI-evaluated justification (anti-idea-hopping)
- Daily logs are upsert (one per user per day)
- Streaks: consecutive days tracked for DeepWork, Gym, Learning, Sober
- Inactivity detection: background job warns after 7+ days of no logging
- Todo categories: Work, Personal, Gym, Learning, Health, Finance, Social, Other

## Auth (MVP)
Currently uses hardcoded `X-User-Id` header. Replace with real JWT auth before production.

## Running Locally
```bash
docker compose up --build
```
- Frontend: http://localhost:3000
- API: http://localhost:5001 (Swagger at /swagger)
- DB: localhost:5432 (postgres/postgres)
