# Personal Execution OS - 30-Day MVP Roadmap

## Week 1: Foundation (Days 1-7)

### Day 1-2: Environment Setup
- [ ] Set up PostgreSQL locally or via Docker
- [ ] Run `001_InitialSchema.sql` to create all tables
- [ ] Configure .NET 8 project, restore NuGet packages
- [ ] Verify API starts and Swagger UI loads at `/swagger`
- [ ] Set up Next.js frontend, install dependencies, verify dev server

### Day 3-4: Core Backend - Goals & Projects
- [ ] Test Goals API: create, get active, complete, abandon
- [ ] Test Projects API: create (verify 2-project limit), update status
- [ ] Test project change request flow with AI gatekeeper
- [ ] Write basic integration tests for constraint enforcement

### Day 5-6: Core Backend - Daily Logs & Streaks
- [ ] Test Daily Logs API: create, update, get by date range
- [ ] Verify streak calculation logic (consecutive days, reset on miss)
- [ ] Test unique constraint (one log per user per day, upsert behavior)
- [ ] Verify all four streak types track independently

### Day 7: Dashboard & Warnings
- [ ] Test Dashboard aggregation endpoint
- [ ] Verify execution score calculation
- [ ] Test inactivity detection background job manually
- [ ] Verify warning creation and acknowledgment flow

## Week 2: Frontend Core (Days 8-14)

### Day 8-9: Dashboard Page
- [ ] Wire up Dashboard page to real API
- [ ] Implement goal progress bar
- [ ] Implement execution score ring
- [ ] Display active projects, streaks, today's log
- [ ] Style warning banners

### Day 10-11: Goal & Project Pages
- [ ] Wire up Goal page: create, complete, abandon flows
- [ ] Wire up Projects page: create, status change, change request
- [ ] Test the 2-project limit enforcement in UI
- [ ] Style the AI gatekeeper response display

### Day 12-13: Daily Log Page
- [ ] Wire up daily log form to API
- [ ] Implement toggle buttons for gym/alcohol
- [ ] Build 30-day heatmap visualization
- [ ] Display streak bars with current/longest counts

### Day 14: Weekly Review Page
- [ ] Wire up review generation button
- [ ] Display structured review sections
- [ ] Show expandable full AI response
- [ ] List historical reviews

## Week 3: AI & Polish (Days 15-21)

### Day 15-16: AI Integration
- [ ] Set up OpenAI API key in configuration
- [ ] Test weekly review generation with real data
- [ ] Test project change gatekeeper with various justifications
- [ ] Tune prompts based on response quality

### Day 17-18: Inactivity System
- [ ] Configure background job interval for testing (shorter than 24h)
- [ ] Test stale project detection
- [ ] Test no-log-for-7-days detection
- [ ] Verify warnings don't duplicate

### Day 19-20: UX Polish
- [ ] Add loading states to all pages
- [ ] Add error handling with user-friendly messages
- [ ] Responsive layout tweaks
- [ ] Keyboard shortcuts for daily log (optional)

### Day 21: Testing
- [ ] End-to-end flow: set goal → add projects → log daily → generate review
- [ ] Test edge cases: abandon goal, switch projects, break streaks
- [ ] Fix any bugs found

## Week 4: Deploy & Harden (Days 22-30)

### Day 22-23: Authentication
- [ ] Implement JWT auth (register/login endpoints)
- [ ] Replace X-User-Id header with JWT extraction
- [ ] Add auth middleware to all controllers
- [ ] Update frontend API client with token management

### Day 24-25: Docker & Deployment
- [ ] Test full docker-compose up flow
- [ ] Verify database initialization
- [ ] Test API-frontend-DB communication in containers
- [ ] Set up environment variable management

### Day 26-27: Data Validation & Security
- [ ] Add input validation on all endpoints
- [ ] Add rate limiting
- [ ] Sanitize AI prompt inputs
- [ ] Review for SQL injection / XSS vectors

### Day 28-29: Final Polish
- [ ] Add favicon and metadata
- [ ] Mobile-responsive testing
- [ ] Performance check (API response times)
- [ ] Write setup instructions in README

### Day 30: Ship
- [ ] Deploy to chosen platform (Railway/Fly.io/VPS)
- [ ] Verify production environment works
- [ ] Start using it daily
- [ ] Log your first day

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
