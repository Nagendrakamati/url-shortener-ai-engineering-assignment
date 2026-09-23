# Project Context

This file is concise continuity context for engineers and AI assistants. Update it when verified project facts or decisions change; do not store secrets here.

## Assignment Context

- Build a working URL shortener prototype in 2-3 days.
- Demonstrate requirement analysis, decomposition, implementation, validation, documentation, risk control, and engineer-owned AI assistance.
- Required stack: ASP.NET Core 10, EF Core 10, SQL Server, Redis, Swagger, Serilog, FluentValidation, xUnit, and Docker.

## Current State

- Phases 1-8 are implemented and validated within the available environment.
- URL creation, redirect, and aggregate analytics APIs are working.
- Redis cache-aside, production concerns, automated tests, Docker assets, and final delivery documentation are implemented.
- The solution is pinned to .NET SDK 10.0.401 through `global.json` and uses .NET/EF Core servicing release 10.0.12.
- SQL Server LocalDB instance `(localdb)\MSSQLLocalDB` was used for disposable validation databases.
- Docker Desktop 29.8.0 and Compose 5.5.1 are installed per-user and operational. The full Compose stack is runtime-validated.
- Microsoft Garnet 2.1.8 is installed outside the repository at `%LOCALAPPDATA%\UrlShortenerTools\Garnet\2.1.8` for local Redis-protocol validation. Start its `net10.0\GarnetServer.exe` with `--bind 127.0.0.1 --port 6379 --lua`; Lua is required by the ASP.NET distributed-cache provider.

## Project Structure

```text
src/UrlShortener.Api
|- Contracts/
|  |- Requests/
|  `- Responses/
|- Controllers/
|- Data/
|  `- Migrations/
|- Middleware/
|- Models/
|- Options/
|- Repositories/
|  `- Interfaces/
|- Services/
|  `- Interfaces/
`- Validation/

tests/UrlShortener.Tests
```

## Verified Architecture

- Dependency flow: Controller -> Service -> Repository -> EF Core -> SQL Server.
- Controllers own HTTP behavior.
- Services own generation, retry, and redirect orchestration.
- Repositories own EF Core, SQL Server, and Redis cache-aside behavior.
- Interfaces are co-located with their service or repository implementations.
- SQL Server is authoritative; Redis caches only short-code destination URLs.
- This is one deployable project with logical three-tier separation.

## Key Decisions

- Use random Base62 generation, not sequential-ID Base62 encoding.
- Generate eight-character codes from 62 symbols using `RandomNumberGenerator`.
- Enforce uniqueness through SQL Server's case-sensitive unique index.
- Retry unique collisions up to five times; propagate unrelated database failures.
- Await analytics updates before returning `302` for simple, reliable assignment-scale behavior.
- Keep aggregate analytics in the `ShortUrls` row; no event table, queue, or background worker.
- Use a domain-specific repository rather than a generic repository.
- Cache successful destination lookups under `short-url:{shortCode}` for 60 minutes.
- Treat Redis reads and writes as best-effort operations; log failures and continue through SQL Server.
- Do not cache unknown short codes or analytics values.
- Validate destination URLs before service execution: required, maximum 2,048 characters, absolute HTTP or HTTPS.
- `UrlsController` explicitly invokes `IValidator<CreateShortUrlRequest>`; automatic MVC FluentValidation is disabled to avoid duplicate validation.
- Return sanitized Problem Details for unexpected errors and correlate them with structured internal logs using a trace ID.
- Controllers log short codes and outcomes but never destination URLs or request bodies.
- Services log short-code collision retries, retry exhaustion, resolution/analytics misses, and visit recording; unexpected exceptions are logged once by the global handler.
- Treat SQL as a readiness dependency and Redis as an optional performance dependency.
- Keep development credentials out of committed configuration; LocalDB uses Windows authentication and containers will use environment overrides.
- Keep migration-on-startup disabled by default; Compose enables it only for its single API replica.
- Omit `design.md` because the assignment currently has no UI.

## Current APIs

- `POST /api/urls`: creates and persists a short URL; returns `201 Created`.
- `GET /{shortCode}`: records a visit and returns `302 Found`, or `404` when missing.
- `GET /api/urls/{shortCode}/analytics`: reads authoritative aggregate analytics from SQL and returns `200`, or `404` when missing.
- Swagger UI is enabled in Development at `/swagger`.
- `GET /health/live`: process liveness.
- `GET /health/ready`: SQL-backed readiness.

## Data Model

- `Id`: bigint identity primary key.
- `ShortCode`: `varchar(10)`, case-sensitive binary collation, unique index.
- `OriginalUrl`: `nvarchar(2048)`.
- `CreatedAtUtc`: `datetime2`.
- `ClickCount`: bigint with default zero.
- `LastAccessedAtUtc`: nullable `datetime2`.

## Validation Evidence

- Solution builds with zero warnings and errors.
- Initial migration applies successfully and matches the EF model snapshot.
- Live creation returned `201` with an eight-character code.
- Live redirect returned `302` with the expected destination.
- Unknown redirect returned `404`.
- SQL showed `ClickCount = 1` and a populated UTC access time after one redirect.
- Repository refactor preserved end-to-end behavior.
- Redis package and cache-aside implementation build with zero warnings and errors.
- With Redis unavailable, cache failures were logged while creation, SQL fallback, redirect, and synchronous analytics all succeeded.
- Live cache-hit behavior was validated against Microsoft Garnet 2.1.8: the expected key/data/TTL existed, redirect returned `302`, analytics updated, and no destination SQL query ran.
- Repeat cache integration against the official Redis container during Phase 8; Garnet is a local protocol-compatible test server, not the deployment choice.
- After two redirects, analytics returned `200` with `ClickCount = 2`, the expected URL and timestamps; an unknown code returned `404`.
- FluentValidation returned `400` for empty, relative, FTP, and over-length URLs with clear messages.
- Serilog emitted structured request completion events and full internal exception records.
- Healthy liveness/readiness returned `200`; simulated SQL failure returned liveness `200`, readiness `503`, and sanitized `500` Problem Details with a trace ID.
- The final checked-in LocalDB configuration produced `201` creation, `200` health probes, and `200` Swagger without environment-based SQL credentials.
- The API and test projects target `net10.0`; restore/build completed without warnings, and EF Core 10.0.12 reported no model drift.
- The full xUnit suite passed 27/27 tests on .NET 10, including real LocalDB repository tests and HTTP contract tests; coverage measured 87.16% lines and 71.87% branches.
- NuGet auditing reported no known vulnerable direct or transitive packages after the .NET 10 migration.
- The Docker image built with pinned .NET 10 Noble images and the Microsoft `dotnet-public` package feed. Compose started the API, SQL Server, and official Redis containers; all three reported healthy, and liveness/readiness returned `200`.

## Next Task

Perform final human sign-off.

## Documentation Map

- `docs/prd.md`: requirements, scope, assumptions, and acceptance criteria.
- `docs/architecture.md`: components, flows, decisions, and trade-offs.
- `docs/rules.md`: coding, security, testing, documentation, and AI-use rules.
- `docs/tasks.md`: phased work, status, acceptance criteria, and validation evidence.
- `docs/Memory.md`: concise verified context for continuity.
- `docs/AI-Usage.md`: AI-assisted execution traceability and controls.
- `docs/Final-Engineering-Summary.md`: final artifacts, decisions, risks, and limitations.