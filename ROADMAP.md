# Personal Execution OS - 30-Day MVP Roadmap

## Week 1: Foundation (Days 1-7)

### Day 1-2: Environment Setup
- [x] Set up PostgreSQL locally or via Docker
- [x] Run schema/migrations to create all tables
- [x] Configure .NET 8 project, restore NuGet packages
- [x] Verify API starts and Swagger UI loads at `/swagger`
- [x] Set up Next.js frontend, install dependencies, verify dev server

### Day 3-4: Core Backend - Goals & Projects
- [x] Test Goals API: create, get active, complete, abandon
- [x] Test Projects API: create (verify 2-project limit), update status
- [x] Test project change request flow with AI gatekeeper
- [x] Write basic integration tests for constraint enforcement

### Day 5-6: Core Backend - Daily Logs & Streaks
- [x] Test Daily Logs API: create, update, get by date range
- [x] Verify streak calculation logic (consecutive days, reset on miss)
- [x] Test unique constraint (one log per user per day, upsert behavior)
- [x] Verify all four streak types track independently

### Day 7: Dashboard & Warnings
- [x] Test Dashboard aggregation endpoint
- [x] Verify execution score calculation
- [x] Test inactivity detection background job manually
- [x] Verify warning creation and acknowledgment flow

## Week 2: Frontend Core (Days 8-14)

### Day 8-9: Dashboard Page
- [x] Wire up Dashboard page to real API
- [x] Implement goal progress bar
- [x] Implement execution score ring
- [x] Display active projects, streaks, today's log
- [x] Style warning banners

### Day 10-11: Goal & Project Pages
- [x] Wire up Goal page: create, complete, abandon flows
- [x] Wire up Projects page: create, status change, change request
- [x] Test the 2-project limit enforcement in UI
- [x] Style the AI gatekeeper response display

### Day 12-13: Daily Log Page
- [x] Wire up daily log form to API
- [x] Implement toggle buttons for gym/alcohol
- [ ] Build 30-day heatmap visualization
- [x] Display streak bars with current/longest counts

### Day 14: Weekly Review Page
- [x] Wire up review generation button
- [x] Display structured review sections
- [x] Show expandable full AI response
- [x] List historical reviews

## Week 3: AI & Polish (Days 15-21)

### Day 15-16: AI Integration
- [x] Set up OpenAI API key in configuration
- [x] Test weekly review generation with real data
- [x] Test project change gatekeeper with various justifications
- [ ] Tune prompts based on response quality

### Day 17-18: Inactivity System
- [x] Configure background job interval for testing (shorter than 24h)
- [x] Test stale project detection
- [x] Test no-log-for-7-days detection
- [x] Verify warnings don't duplicate

### Day 19-20: UX Polish
- [ ] Add loading states to all pages
- [x] Add error handling with user-friendly messages
- [ ] Responsive layout tweaks
- [ ] Keyboard shortcuts for daily log (optional)

### Day 21: Testing
- [x] End-to-end flow: set goal → add projects → log daily → generate review
- [x] Test edge cases: abandon goal, switch projects, break streaks
- [x] Fix any bugs found

## Week 4: Deploy & Harden (Days 22-30)

### Day 22-23: Authentication
- [x] Implement Google OAuth authentication (replaces basic JWT register/login)
- [x] Extract user identity from JWT claims (replaced X-User-Id header)
- [x] Add `[Authorize]` middleware to all controllers
- [x] Update frontend API client with token management

### Day 24-25: Docker & Deployment
- [x] Test full docker-compose up flow
- [x] Verify database initialization
- [x] Test API-frontend-DB communication in containers
- [x] Set up environment variable management

### Day 26-27: Data Validation & Security
- [ ] Add input validation on all endpoints
- [ ] Add rate limiting
- [ ] Sanitize AI prompt inputs
- [ ] Review for SQL injection / XSS vectors

### Day 28-29: Final Polish
- [ ] Add favicon and metadata
- [ ] Mobile-responsive testing
- [ ] Performance check (API response times)
- [x] Write setup instructions in README

### Day 30: Ship
- [ ] Deploy to chosen platform (Railway/Fly.io/VPS)
- [ ] Verify production environment works
- [ ] Start using it daily
- [ ] Log your first day

---

## Additional Features Implemented (Beyond Original Roadmap)

- [x] **Todos System** — Full CRUD with priority, due dates, goal/project linking (`TodosController`, `TodoService`, `TodoItem` model, `/todos` page)
- [x] **Google OAuth** — Social sign-in via Google replaces traditional email/password auth
- [x] **Unit Test Suite** — 61 xUnit tests covering Goals, Projects, DailyLogs, Streaks, Warnings, WeeklyReview, AI parsing, and InactivityDetection

---

## Post-MVP Features (Backlog)

- Mobile PWA support
- Notification system (email/push for inactivity warnings)
- Data export (CSV/JSON)
- Goal templates
- Multi-user support with privacy
- Advanced analytics (weekly/monthly trends)
- Calendar integration
- Pomodoro timer integration for deep work tracking
