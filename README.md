# Planning Backend

## Running Locally

To run the backend locally without Docker, ensure you have the .NET 10.0 SDK installed.

1. Navigate to the API project directory:
   ```bash
   cd backend/PlanBackend.Api
   ```

2. Run the application:
   ```bash
   dotnet run
   ```
   
   *Note: The SQLite database (`plans.db`) will be automatically created in this directory upon application startup.*

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

## Core Server Integration (Feature Flag)

The backend interacts with the game-room (Core server) for two purposes:
1. **Validation**: Querying the game-room to verify if units and resources actually exist before accepting a plan.
2. **Notification**: Notifying the game-room when a plan has been updated.

The backend supports two distinct architectures for this integration, toggleable via the `USE_REDIS` feature flag.

- **HTTP (Default)**: If `USE_REDIS` is set to `false`, the backend relies entirely on HTTP calls to the `CoreBaseUrl`. It queries state via `GET /game-state` and sends notifications via `POST /plan-updated`.
- **Redis State Mirroring & PubSub**: If `USE_REDIS` is set to `true`, the backend completely decouples from direct game-room HTTP requests.
  - **Validation**: It reads valid unit and resource IDs directly from Redis Sets (e.g., `game:<game-room-id>:units`).
  - **Notification**: It publishes a JSON payload to a Redis channel using the pattern `planning.<game-room-id>.plan-updated`. 

### Configuration

You can configure the feature flag and Redis connection string in `backend/PlanBackend.Api/appsettings.json`:

```json
{
  "USE_REDIS": true,
  "RedisConnection": "localhost:6379"
}
```

Or you can override it using environment variables when running via Docker:

```bash
docker run -p 5000:8080 \
  -e UseRedisNotifier=true \
  -e RedisConnection=redis:6379 \
  planbackend-api
```

When using `docker-compose.yml`, the application is already configured to start a local Redis container and use the Redis notification architecture by default.