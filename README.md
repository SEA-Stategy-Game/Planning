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


## Core Server Integration (Feature Flag)

The backend interacts with the game-room (Core server) for two purposes:
1. **Validation**: Querying the game-room to verify if units and resources actually exist before accepting a plan.
2. **Notification**: Notifying the game-room when a plan has been updated.

The backend supports two distinct architectures for this integration, toggleable via the `UseRedis` feature flag.

- **HTTP (Default)**: If `UseRedis` is set to `false`, the backend relies entirely on HTTP calls to the `CoreBaseUrl`. It queries state via `GET /game-state` and sends notifications via `POST /plan-updated`.
- **Redis State Mirroring & PubSub**: If `UseRedis` is set to `true`, the backend completely decouples from direct game-room HTTP requests.
  - **Validation**: It reads valid unit and resource IDs directly from Redis Sets (e.g., `game:<game-room-id>:units`).
  - **Notification**: It publishes a JSON payload to a Redis channel using the pattern `planning.<game-room-id>.plan-updated`. 


### Configuration

If you are using HTTP connection to Core, the URL can be set a follows:

`PlanBackend.Api/appsettings.json`:

```json
{
  "CoreBaseUrl": "http://127.0.0.1:8085"
}
```

`CoreBaseUrl` is used to notify Core after a plan is persisted, and for entity-existence validation via `CoreSenseClient`. If Core is unavailable both operations fail silently — the backend continues to function.



You can configure the feature flag and Redis connection string in `backend/PlanBackend.Api/appsettings.json`:

```json
{
  "UseRedis": true,
  "RedisConnection": "localhost:6379"
}
```

Or you can override it using environment variables when running via Docker:

```bash
docker run -p 5000:8080 \
  -e UseRedis=true \
  -e RedisConnection=redis:6379 \
  planbackend-api
```



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



## Running using Docker

To run the backend using Docker, execute the following commands from the **root of the repository**:

1. Build the Docker image:
   ```bash
   docker build -t planbackend-api .
   ```

2. Run the Docker container (maps localhost port 5000 to container port 8080):
   ```bash
   docker run -p 5000:8080 planbackend-api
   ```
   
   *Note: You can override the Core Service URL by supplying the `CoreBaseUrl` environment variable:*
   `docker run -e CoreServiceUrl=http://host.docker.internal:8085 -p 5000:8080 planbackend-api`

### Persisting the Database Externally

By default, the SQLite database is created inside the container and will be lost when the container is stopped. To persist the data locally, mount a directory and override the database connection string:

```bash
# 1. Create a local data directory
mkdir -p data

# 2. Run the container with a volume mount and connection string override
docker run -p 5000:8080 \
  -v $(pwd)/data:/app/data \
  -e ConnectionStrings__DefaultConnection="Data Source=data/plans.db" \
  planbackend-api
```




When using `docker-compose.yml`, the application is already configured to start a local Redis container and use the Redis notification architecture by default.