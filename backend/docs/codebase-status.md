# Codebase Status

Generated: 2026-03-05

---

## PlanBackend.Domain

No external dependencies. All types are plain C# — no framework coupling.

### GamePlan
- **Status:** Implemented
- **Responsibility:** Aggregate root representing one versioned submission of all unit plans for a (GameId, PlayerId) pair.
- **Architectural role:** Domain model, no dependencies. Owned by the Domain layer. Used by Application interfaces and Infrastructure serialisation.
- **Notes:** None.

### UnitPlan
- **Status:** Implemented
- **Responsibility:** Holds the ordered list of steps for a single unit within a GamePlan.
- **Architectural role:** Domain model, child of GamePlan.
- **Notes:** None.

### PlanStep
- **Status:** Implemented
- **Responsibility:** Represents one atomic instruction (action, conditional, or loop) within a UnitPlan.
- **Architectural role:** Domain model, child of UnitPlan. `Parameters` is `Dictionary<string, string>` — matches the IR DTO.
- **Notes:** None.

### StepType (enum)
- **Status:** Implemented
- **Responsibility:** Discriminates between step kinds: `Action`, `Conditional`, `Loop`.
- **Architectural role:** Domain enum, referenced by `PlanStep`.
- **Notes:** None.

---

## PlanBackend.Application

Depends only on Domain. Contains interfaces, service logic, and cross-layer DTOs.

### IPlanRepository
- **Status:** Implemented
- **Responsibility:** Defines the persistence contract — save a plan, batch-retrieve unit plans by unitId list, get history, get a specific version.
- **Architectural role:** Application interface, implemented by `Infrastructure.PlanRepository`. Keeps Application free of EF Core.
- **Notes:** `GetUnitPlansAsync(gameId, playerId, unitIds: List<string>)` replaces the former single-unit `GetLatestUnitPlanAsync`. Returns all matching active `UnitPlan`s in one query.

### ICoreNotifier
- **Status:** Implemented
- **Responsibility:** Defines the contract for notifying Core that new plans are ready to be fetched.
- **Architectural role:** Application interface, implemented by `Infrastructure.CoreNotifier`.
- **Notes:** Signature `NotifyPlanUpdatedAsync(gameId, playerId, unitIds)` takes a list of unit IDs — differs from CLAUDE.md which describes individual per-unit POST bodies. Coordinate with Core group before finalising.

### PlanService
- **Status:** Partial stub — `SubmitPlanAsync`, `GetPlanHistoryAsync`, `GetPlanVersionAsync` throw `NotImplementedException`; `GetUnitPlansAsync` validates arguments and delegates to repository.
- **Responsibility:** Orchestrates plan submission (validate → save → notify) and retrieval queries.
- **Architectural role:** Application service, consumed directly by `Api.PlanController`. Depends on `IPlanRepository` and `ICoreNotifier`.
- **Notes:** `PlanController` depends on the concrete `PlanService` class rather than an interface, making unit testing harder. Not a dependency-rule violation, but consider extracting `IPlanService` if the controller needs to be tested in isolation.

### GamePlanSummary
- **Status:** Implemented
- **Responsibility:** Lightweight read DTO returned by history queries — version, date, active flag, unit count.
- **Architectural role:** Application DTO, returned by `IPlanRepository.GetGamePlanHistoryAsync`.
- **Notes:** None.

### SubmitResult
- **Status:** Implemented
- **Responsibility:** Response DTO from a plan submission — success flag, new version number, which unit IDs changed, and any validation errors.
- **Architectural role:** Application DTO, returned by `PlanService.SubmitPlanAsync` and ultimately by `PlanController.PostPlan`.
- **Notes:** None.

---

## PlanBackend.Infrastructure

Depends on Application (for interfaces) and EF Core / SQLite. Implements persistence and HTTP notification.

### PlanDbContext
- **Status:** Implemented
- **Responsibility:** EF Core `DbContext` exposing `GamePlans` and `UnitPlans` DbSets.
- **Architectural role:** Infrastructure, registered as scoped in `Program.cs`. Used by `PlanRepository`.
- **Notes:** No `OnModelCreating` configuration — EF will infer all mappings by convention. Explicit configuration may be needed (indexes, FK relationship between `UnitPlanEntity.GamePlanId` and `GamePlanEntity.Id`). No EF migrations have been created yet.

### GamePlanEntity
- **Status:** Implemented
- **Responsibility:** EF entity storing one version of a full plan as a JSON blob (`GamePlanJson`) alongside scalar metadata.
- **Architectural role:** Infrastructure persistence entity, mapped to the `GamePlans` table.
- **Notes:** No navigation property to `UnitPlanEntity` — join is done manually in repository code. That is fine for the JSON-blob strategy.

### UnitPlanEntity
- **Status:** Implemented
- **Responsibility:** EF entity storing one row per unit per plan version for fast by-unitId lookups. `ContentHash` enables change detection.
- **Architectural role:** Infrastructure persistence entity, mapped to the `UnitPlans` table.
- **Notes:** `GamePlanId` is a foreign key to `GamePlanEntity.Id` but no EF navigation property or FK constraint is configured — needs `OnModelCreating` or data annotation.

### PlanRepository
- **Status:** Stub — all four methods throw `NotImplementedException`
- **Responsibility:** Will implement `IPlanRepository` using EF Core: save/query `GamePlanEntity` and `UnitPlanEntity` rows, including a batch query for `GetUnitPlansAsync` using `WHERE unitId IN (...)`.
- **Architectural role:** Infrastructure, registered as scoped in `Program.cs` against `IPlanRepository`.
- **Notes:** **Critical gap — `PlanDbContext` is not injected.** The class has no constructor and therefore cannot access the database. A constructor accepting `PlanDbContext` must be added before any method can be implemented.

### CoreNotifier
- **Status:** Stub — throws `NotImplementedException`
- **Responsibility:** Will send an HTTP POST to the Core service when new plans are ready.
- **Architectural role:** Infrastructure, registered via `AddHttpClient<CoreNotifier>()` in `Program.cs`. Implements `ICoreNotifier`.
- **Notes:** Core service base URL is not yet configured — will need an `appsettings.json` entry and `IConfiguration` or `IOptions<>` injection. The exact POST body and Core endpoint URL are defined in the Contracts Repo.

---

## PlanBackend.Api

Depends on Infrastructure and Application. Hosts the HTTP surface: controller, DTOs, DI wiring.

### Program.cs
- **Status:** Implemented (DI wiring and routing)
- **Responsibility:** Bootstraps the application, registers services, maps the health endpoint and MVC controllers.
- **Architectural role:** Composition root. Registers `PlanDbContext`, `PlanRepository`, `CoreNotifier`, `PlanService`, and MVC.
- **Notes:** `EnsureCreatedAsync()` or `MigrateAsync()` is not called — the SQLite database file will not be created on startup. Must be added before the app can persist anything.

### PlanController
- **Status:** Partial stub — `PostPlan`, `GetPlanHistory`, `GetPlanVersion` return hardcoded responses; `GetUnitPlans` is wired to `_service.GetUnitPlansAsync` (will throw until repository is implemented).
- **Responsibility:** Exposes four HTTP endpoints: `POST /plan`, `GET /plan/{gameId}/{playerId}?unitIds=u1,u2`, `GET /plan/{gameId}/{playerId}/history`, `GET /plan/{gameId}/{playerId}/version/{version}`.
- **Architectural role:** API layer, depends on `PlanService`. Responsible for mapping IR DTOs → domain models and domain results → HTTP responses.
- **Notes:** `GetUnitPlans` parses the comma-separated `unitIds` query parameter, validates it is non-empty, calls the service, and maps results to `List<UnitPlanResponse>`. Returns 400 if `unitIds` is absent/empty, 404 if the service returns an empty list.

### PlanSubmissionIR
- **Status:** Implemented
- **Responsibility:** JSON deserialization target for incoming plan submissions. Top-level payload with `schema_version`, `game_id`, `player_id`, `unit_plans`.
- **Architectural role:** API-layer DTO. Mapped to `GamePlan` domain model in the controller before calling the service.
- **Notes:** CLAUDE.md IR schema still shows the old `target: {type, id}` / `plan_type` shape — the actual DTO uses `unit_id` directly. CLAUDE.md needs updating.

### UnitPlanIR
- **Status:** Implemented
- **Responsibility:** IR DTO for a single unit's plan within a submission — `unit_id` and a list of steps.
- **Architectural role:** Nested inside `PlanSubmissionIR`. Mapped to `UnitPlan`.
- **Notes:** None.

### PlanStepIR
- **Status:** Implemented
- **Responsibility:** IR DTO for one step — `step_index`, `step_type`, `action_type`, `parameters`.
- **Architectural role:** Nested inside `UnitPlanIR` (inbound) and `UnitPlanResponse` (outbound). Mapped to/from `PlanStep`.
- **Notes:** None.

### UnitPlanResponse
- **Status:** Implemented
- **Responsibility:** Response DTO for the batch unit-plan endpoint — `unit_id` and a list of `PlanStepIR` steps.
- **Architectural role:** API-layer response DTO, returned as `List<UnitPlanResponse>` by `GET /plan/{gameId}/{playerId}?unitIds=...`.
- **Notes:** Reuses `PlanStepIR` for step serialisation.

---

## PlanBackend.Tests

### (no test files)
- **Status:** Empty — project scaffolded with xUnit, no test classes written.
- **Notes:** References `Application` and `Domain` but not `Infrastructure` or `Api`. Integration tests will need additional package references (`Microsoft.AspNetCore.Mvc.Testing`, EF Core InMemory or SQLite in-memory).
