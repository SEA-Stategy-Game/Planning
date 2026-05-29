# Planning — AI & Planning (Group 4)

This repository contains the ASP.NET Core Plan Backend (C#). The DSL binary is bundled with the Godot client.

---

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 8.0+ |
| F# (included with .NET SDK) | — |

Verify with:

```bash
dotnet --version
```

---

## Running the Plan Backend

```bash
cd backend
dotnet run --project PlanBackend.Api
```

The server starts on **http://localhost:5020** by default.

The SQLite database (`plans.db`) is created automatically on first run inside `PlanBackend.Api/`.

### Configuration

`PlanBackend.Api/appsettings.json`:

```json
{
  "CoreBaseUrl": "http://127.0.0.1:8085"
}
```

`CoreBaseUrl` is used to notify Core after a plan is persisted, and for entity-existence validation via `CoreSenseClient`. If Core is unavailable both operations fail silently — the backend continues to function.

---

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/plan` | Submit a plan (JSON IR) |
| `GET` | `/plan/{gameId}/{playerId}/history` | List plan versions for a player |
| `GET` | `/plan/{gameId}/{playerId}/version/{n}` | Retrieve a specific version |
| `GET` | `/plan/{gameId}/{playerId}?unitIds=u1,u2` | Batch fetch active unit plans |
| `GET` | `/health` | Health check |

### Example: Submit a plan

```bash
curl -X POST http://localhost:5020/plan \
  -H "Content-Type: application/json" \
  -d @backend/docs/plan_json_client_submit_plan_to_backend.json
```

---

## Running Tests

```bash
cd backend
dotnet test
```

Three test suites are included:

- `PlanServiceTests` — unit tests with mocked repository and notifier
- `PlanRepositoryTests` — integration tests against an in-memory SQLite database
- `PlanControllerTests` — end-to-end tests using `WebApplicationFactory`

---

## Port reference

| Service | Port |
|---------|------|
| Plan Backend | 5020 |
| Core System | 8085 |
