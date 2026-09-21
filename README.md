# URL Shortener

A production-conscious URL shortener prototype built for the Charles Schwab AI-Proficient Software Engineer assignment. It demonstrates engineer-led AI-assisted delivery, a pragmatic three-tier architecture, SQL-backed correctness, Redis cache-aside behavior, aggregate analytics, validation, structured logging, health checks, and automated tests.

## Architecture

```text
Client -> Controller -> Service -> Repository -> SQL Server
                                      |
                                      `-> Redis cache
```

- SQL Server is the source of truth.
- Redis caches only short-code destination lookups for 60 minutes.
- Redirect analytics are updated synchronously and atomically in SQL Server.
- Redis failures fall back to SQL and do not affect correctness.
- The application is one deployable ASP.NET Core 10 API with logical tiers.

See [docs/architecture.md](docs/architecture.md) for request flows and decisions.

## Quick Start With Docker

Docker is the recommended way to run the project. It starts the API, SQL Server, and Redis together, so no separate .NET or database installation is required.

### 1. Install And Start Docker Desktop

- Install [Docker Desktop](https://www.docker.com/products/docker-desktop/).
- Start Docker Desktop and wait until it reports that the Docker engine is running.
- Keep Docker Desktop open while using the project.

Hardware virtualization must be enabled on Windows. If Docker Desktop displays `Virtualization support not detected`, enable virtualization in BIOS/UEFI or ask your administrator to enable it.

### 2. Open The Project Folder

Extract the submitted ZIP file. Open PowerShell in the extracted folder that contains `compose.yaml` and `Dockerfile`.

For example:

```powershell
cd "C:\path\to\UrlShortener-Assignment"
```

### 3. Create The Environment File

Copy the provided example file:

```powershell
Copy-Item .env.example .env
```

Open `.env` in a text editor and replace the example `SQL_SA_PASSWORD` value with a strong local password containing uppercase, lowercase, numeric, and special characters. Do not share or commit `.env`.

The file should keep this structure:

```dotenv
SQL_SA_PASSWORD=Your-Strong-Local-Password-123!
API_PORT=5080
SQL_PORT=1433
REDIS_PORT=6379
```

### 4. Build And Start The Project

Run these commands from the same project folder:

```powershell
docker compose up --build --detach
docker compose ps
```

The first run downloads the required images and can take several minutes. Wait until `api`, `sqlserver`, and `redis` all show `healthy` in the `docker compose ps` output.

Compose waits for SQL Server and Redis before starting the API. The API then applies the checked-in database migration automatically.

### 5. Open And Verify The Application

Open:

- Swagger: http://localhost:5080/swagger
- Liveness: http://localhost:5080/health/live
- SQL readiness: http://localhost:5080/health/ready

Both health pages should display `Healthy`. If `API_PORT` was changed in `.env`, replace `5080` in these URLs with that value.

### Troubleshooting

If SQL Server is unhealthy and its logs say that the password did not match, an existing Docker volume was created with a different password. Reset the local database and start again:

```powershell
docker compose down --volumes
docker compose up --build --detach
docker compose ps
```

Warning: `docker compose down --volumes` deletes all URL data stored by this local Compose project.

If a port is already in use, change `API_PORT`, `SQL_PORT`, or `REDIS_PORT` in `.env`, then restart the stack.

### Smoke Test

```powershell
$created = Invoke-RestMethod `
  -Uri "http://localhost:5080/api/urls" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"url":"https://www.schwab.com"}'

$created
curl.exe -i --max-redirs 0 "http://localhost:5080/$($created.shortCode)"
Invoke-RestMethod "http://localhost:5080/api/urls/$($created.shortCode)/analytics"
```

### Logs And Shutdown

```powershell
docker compose logs --follow api
docker compose down
```

To remove the SQL Server development volume and all local URL data:

```powershell
docker compose down --volumes
```

## Local Development Without Docker

Prerequisites:

- .NET SDK `10.0.401` (pinned by `global.json`)
- SQL Server LocalDB instance `(localdb)\MSSQLLocalDB`
- Redis-compatible server on `localhost:6379` (optional because SQL fallback is supported)

Initialize and run:

```powershell
dotnet tool restore
dotnet ef database update `
  --project .\src\UrlShortener.Api `
  --startup-project .\src\UrlShortener.Api

$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project .\src\UrlShortener.Api --urls http://localhost:5080
```

Swagger is available at http://localhost:5080/swagger.

## API

| Method | Route | Result |
|---|---|---|
| `POST` | `/api/urls` | Creates a short URL and returns `201` |
| `GET` | `/{shortCode}` | Records a visit and returns `302`, or `404` |
| `GET` | `/api/urls/{shortCode}/analytics` | Returns aggregate analytics, or `404` |
| `GET` | `/health/live` | Process liveness |
| `GET` | `/health/ready` | SQL Server readiness |

Create request:

```json
{
  "url": "https://www.schwab.com"
}
```

Only absolute HTTP and HTTPS URLs up to 2,048 characters are accepted.

## Tests

The repository suite uses disposable SQL Server LocalDB databases for relational behavior and does not use EF InMemory.

```powershell
dotnet test .\tests\UrlShortener.Tests\UrlShortener.Tests.csproj
```

Coverage:

```powershell
dotnet test .\tests\UrlShortener.Tests\UrlShortener.Tests.csproj `
  --collect:"XPlat Code Coverage"
```

Verified result: 27 passed, 0 failed, 0 skipped; 87.16% line and 71.87% branch coverage.

## Configuration

ASP.NET configuration supports environment overrides. Important keys:

| Key | Purpose |
|---|---|
| `ConnectionStrings__SqlServer` | Authoritative SQL Server connection |
| `ConnectionStrings__Redis` | Optional Redis cache connection |
| `Cache__ShortUrlExpirationMinutes` | Positive cache TTL |
| `Database__MigrateOnStartup` | Compose-only migration switch; default is `false` |

Production secrets should come from an approved secret manager, not `.env` or source control. For multi-instance production deployment, run migrations as a separate release step rather than enabling migration-on-startup on every replica.

## Documentation

- [Product requirements](docs/prd.md)
- [Architecture](docs/architecture.md)
- [Engineering rules](docs/rules.md)
- [Tasks and validation evidence](docs/tasks.md)
- [Project context](docs/Memory.md)
- [AI usage and traceability](docs/AI-Usage.md)
- [Final engineering summary](docs/Final-Engineering-Summary.md)

## Known Limitations

- No authentication, ownership, custom aliases, expiration, editing, or deletion.
- Analytics are aggregate-only; individual click events are not retained.
- Redirects await the SQL analytics update, trading latency for reliable assignment-scale counts.
- Advanced abuse detection, multi-region deployment, and edge caching are intentionally out of scope.