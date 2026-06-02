# Task List

Last updated: 2026-03-05
See `codebase-status.md` for per-file analysis.

---

## Blocked

### B5 — Implement `CoreNotifier.NotifyPlanUpdatedAsync`
**Blocked by:** Core group must confirm the notification endpoint URL and expected POST body (see Contracts Repo). Until confirmed, the notifier stays a stub.
- **Files:** `backend/PlanBackend.Infrastructure/CoreNotifier.cs`, `backend/PlanBackend.Api/appsettings.json`
- **What:** Read Core base URL from configuration. POST the notification payload to the Core endpoint. Handle transient HTTP errors gracefully — a Core failure must not roll back a successful plan persistence.
- **Acceptance criteria:** After a successful `POST /plan`, Core receives an HTTP POST with the correct payload.

### B6 — Write integration tests
**Blocked by:** Nothing — all endpoints are now wired. Needs additional test packages (`Microsoft.AspNetCore.Mvc.Testing`, EF Core SQLite in-memory).
- **Files:** `backend/PlanBackend.Tests/`
- **What:** At minimum: `POST /plan` happy path, `POST /plan` with empty `unit_plans` returns 400, `GET /plan/{gameId}/{playerId}?unitIds=...` returns correct steps after submit, 404 for unknown unit.
- **Acceptance criteria:** All tests pass with `dotnet test`.

---

## Done

- Solution file (`PlanBackend.slnx`) and all five projects scaffolded with correct project references
- Dependency rule enforced: Domain ← Application ← Infrastructure ← Api
- Domain models: `GamePlan`, `UnitPlan`, `PlanStep`, `StepType` enum
- Application interfaces: `IPlanRepository` (4 methods — save, batch-GET, history, get-by-version), `ICoreNotifier`
- Application DTOs: `SubmitResult`, `GamePlanSummary`
- Infrastructure entities: `GamePlanEntity`, `UnitPlanEntity`
- `PlanDbContext` with `GamePlans`/`UnitPlans` DbSets and `OnModelCreating` (FK + composite indexes)
- `CoreNotifier` constructor with `HttpClient` injection (body still stub — see B5)
- API DTOs: `PlanSubmissionIR`, `UnitPlanIR`, `PlanStepIR`, `UnitPlanResponse`
- `PlanController` with all four endpoints fully wired
- `Program.cs` DI wiring + `EnsureCreatedAsync` on startup
- `GET /health` responds 200 `{"status":"ok"}`
- `.gitignore` excluding `bin/`, `obj/`, `.claude/`, `*.db`
- `PlanRepository` fully implemented: `SaveGamePlanAsync` (deactivate old, insert new, SHA-256 content hash per unit), `GetUnitPlansAsync` (batch lookup via active plan JSON), `GetGamePlanHistoryAsync`, `GetGamePlanByVersionAsync`
- `PlanService` fully implemented: `SubmitPlanAsync` (validate, auto-increment version, save), `GetUnitPlansAsync`, `GetPlanHistoryAsync`, `GetPlanVersionAsync`
- `POST /plan` end-to-end: IR → domain mapping, persist to SQLite, return `SubmitResult`
- `GET /plan/{gameId}/{playerId}?unitIds=u1,u2` end-to-end: batch lookup, return `List<UnitPlanResponse>`
- `GET /plan/{gameId}/{playerId}/history` and `/version/{n}` wired to service
