# Execution OS - Project Rules

## Overview
Personal Execution OS — a full-stack accountability app. One 90-day goal, max 2 active projects, daily logging, streak tracking, AI-powered weekly reviews, and daily todos with categories. Google OAuth authentication.

## Tech Stack
- **Frontend**: Next.js 14 (App Router), React 18, TypeScript, Tailwind CSS 3.4
- **Backend**: .NET 8 Web API, C#, Entity Framework Core 8
- **Database**: PostgreSQL 16
- **Auth**: Google OAuth + JWT Bearer tokens
- **AI**: OpenAI GPT-4 (weekly reviews, project change evaluation)
- **Deployment**: Docker Compose

## Project Structure
```
backend/ExecutionOS.API/
  Controllers/    # API controllers (REST, [Authorize] + [ApiController])
  Data/           # AppDbContext (EF Core)
  DTOs/           # Request/Response records
  Jobs/           # Background hosted services
  Migrations/     # Raw SQL migration files (sequential numbered)
  Models/         # EF Core entity classes
  Services/       # Business logic layer
  Program.cs      # DI registration, JWT config, middleware pipeline

frontend/src/
  app/            # Next.js App Router pages (each folder = route)
  app/sign-in/    # Public sign-in page (Google OAuth)
  components/     # Shared React components (AuthGuard, Sidebar, etc.)
  lib/api.ts      # API client functions (fetch + JWT Bearer)
  lib/auth.tsx    # AuthProvider context (login, logout, token storage)
  types/api.ts    # TypeScript interfaces for API responses
```

## Authentication

### Flow
1. User clicks "Sign in with Google" on `/sign-in` page
2. Google Identity Services returns an ID token (credential)
3. Frontend sends ID token to `POST /api/auth/google`
4. Backend validates token via `Google.Apis.Auth`, creates/finds user, returns JWT
5. Frontend stores JWT + user info in `localStorage('auth')`
6. All subsequent API requests include `Authorization: Bearer <token>` header
7. Backend validates JWT on every `[Authorize]` endpoint via `ClaimTypes.NameIdentifier`

### Key Files
- **Backend**: `Services/AuthService.cs` — Google token validation, user upsert, JWT generation
- **Backend**: `Controllers/AuthController.cs` — `POST /api/auth/google` (public), `GET /api/auth/me` (protected)
- **Backend**: `DTOs/AuthDtos.cs` — `GoogleLoginRequest`, `AuthResponse`
- **Frontend**: `lib/auth.tsx` — `AuthProvider`, `useAuth()` hook
- **Frontend**: `components/AuthGuard.tsx` — redirects to `/sign-in` if not authenticated
- **Frontend**: `app/sign-in/page.tsx` — Google sign-in button

### Configuration
- `Auth:GoogleClientId` — Google OAuth Client ID (from Google Cloud Console)
- `Auth:JwtSecret` — HMAC-SHA256 signing key (min 32 chars, change in production)
- `Auth:JwtIssuer` / `Auth:JwtAudience` — both default to `ExecutionOS`
- `NEXT_PUBLIC_GOOGLE_CLIENT_ID` — same Google Client ID, exposed to frontend

### Setup
1. Go to [Google Cloud Console](https://console.cloud.google.com/apis/credentials)
2. Create OAuth 2.0 Client ID (Web application)
3. Add `http://localhost:3000` to authorized JavaScript origins
4. Copy the Client ID
5. Set `GOOGLE_CLIENT_ID` env var (used by both docker-compose services)

## Conventions

### Backend
- **Models**: Classes in `Models/` with `Guid Id`, `Guid UserId`, `DateTime CreatedAt/UpdatedAt`. Enums defined in same file as the model.
- **DTOs**: Records in `DTOs/` — separate `Create*Request`, `Update*Request`, and `*Response` records. One file per domain (e.g., `GoalDtos.cs`).
- **Services**: One service per domain in `Services/`. Inject `AppDbContext` via constructor. Async/await. Throw `InvalidOperationException` for business rule violations. Private `MapToResponse()` method for entity-to-DTO mapping.
- **Controllers**: `[Authorize]` + `[ApiController]` + `[Route("api/[controller]")]`. One controller per domain. `GetUserId()` extracts user ID from JWT `ClaimTypes.NameIdentifier`. Returns `ActionResult<T>`. Only `AuthController` endpoints use `[AllowAnonymous]`.
- **DI**: Register services as `AddScoped<T>()` in `Program.cs`.
- **DB**: Add `DbSet<T>` to `AppDbContext`. Configure relationships in `OnModelCreating` with Fluent API. Enum properties use `.HasConversion<string>()`.
- **Migrations**: Sequential numbered SQL files in `Migrations/` (e.g., `001_InitialSchema.sql`, `002_AddTodos.sql`, `003_AddGoogleAuth.sql`). Also mounted in `docker-compose.yml`.

### Frontend
- **Pages**: `'use client'` components in `app/<route>/page.tsx`. Use `useState` + `useEffect` for data fetching. `load()` function pattern for refresh.
- **API calls**: All go through `lib/api.ts` which wraps `fetch()` with `Authorization: Bearer` header from localStorage. Generic `request<T>()` function. Auto-redirects to `/sign-in` on 401.
- **Auth**: `useAuth()` hook provides `user`, `loading`, `login(idToken)`, `logout()`. `AuthGuard` component in layout handles route protection.
- **Types**: Interfaces in `types/api.ts` matching backend response DTOs (camelCase).
- **Styling**: Tailwind utility classes. Custom color tokens: `bg`, `surface`, `border`, `accent`, `accent-dim`, `warning`, `danger`, `muted`. Component classes in `globals.css`: `.card`, `.btn-primary`, `.btn-danger`, `.btn-secondary`, `.input`, `.label`.
- **Navigation**: Sidebar component with `nav` array of `{ href, label }` objects. Shows user profile pic, name, email, and sign-out button.

## Key Business Rules
- One active 90-day goal per user at a time
- Maximum 2 active projects per user
- Project changes require AI-evaluated justification (anti-idea-hopping)
- Daily logs are upsert (one per user per day)
- Streaks: consecutive days tracked for DeepWork, Gym, Learning, Sober
- Inactivity detection: background job warns after 7+ days of no logging
- Todo categories: Work, Personal, Gym, Learning, Health, Finance, Social, Other

## Running Locally
```bash
# Set your Google Client ID
export GOOGLE_CLIENT_ID=your-google-client-id-here

docker compose up --build
```
- Frontend: http://localhost:3000
- API: http://localhost:5001 (Swagger at /swagger)
- DB: localhost:5432 (postgres/postgres)
