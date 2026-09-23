# Tasks and Progress

Status values: `Not Started`, `In Progress`, `Complete`, `Deferred`.

## Phase 1: Foundation

Status: **Complete**

- [x] Create solution, API project, and xUnit project.
- [x] Add EF Core SQL Server dependencies and local `dotnet-ef` manifest.
- [x] Create `ShortUrl` entity and `UrlShortenerDbContext`.
- [x] Create and verify the initial migration.
- [x] Add a case-sensitive unique index for `ShortCode`.
- [x] Retarget the API and tests to .NET 10 and align EF Core/tooling to 10.0.12.
- Validation: the `net10.0` solution built with zero warnings/errors; EF 10 discovered the existing migration and reported no pending model changes; migration applied successfully to LocalDB.

## Phase 2: URL Creation

Status: **Complete**

- [x] Add `POST /api/urls`.
- [x] Generate cryptographically random eight-character Base62 codes.
- [x] Retry SQL unique-key collisions up to five times.
- [x] Return `201 Created` and a `Location` header.
- Validation: live SQL-backed request returned `201`; generated code and persisted row were verified.

## Phase 3: URL Redirect

Status: **Complete**

- [x] Add `GET /{shortCode}`.
- [x] Return `302` for known codes and `404` for unknown codes.
- [x] Await an atomic click-count and last-access update.
- [x] Refactor into Controller -> Service -> Repository tiers.
- [x] Co-locate service and repository interfaces with their implementations.
- Validation: live request returned `302`; SQL showed `ClickCount = 1`; build passed with zero warnings/errors.

## Phase 4: Redis Cache

Status: **Complete**

- [x] Add Redis dependency and configuration.
- [x] Add cache-aside lookup for short-code resolution.
- [x] Cache successful SQL inserts and SQL lookup results.
- [x] Configure a validated 60-minute cache TTL.
- [x] Fall back to SQL when Redis misses or is unavailable.
- [x] Preserve synchronous SQL analytics updates.
- Validation: build passed with zero warnings/errors; with Redis deliberately unavailable, creation returned `201`, redirect returned `302`, Redis failures were logged, and SQL showed `ClickCount = 1`.
- Cache-hit validation: Microsoft Garnet 2.1.8 provided a local Redis-protocol endpoint. The `short-url:{shortCode}` key existed with the exact destination and an approximately 60-minute TTL; redirect returned `302`; SQL analytics reached `ClickCount = 1`; API logs showed the analytics `UPDATE` without a destination `SELECT`.
- Final environment check: repeat the same integration test against the official Redis container added in Phase 8.

## Phase 5: Analytics API

Status: **Complete**

- [x] Add `UrlAnalyticsResponse` under `Contracts/Responses`.
- [x] Add repository and service analytics queries.
- [x] Add `GET /api/urls/{shortCode}/analytics`.
- [x] Return `404` for an unknown code.
- Validation: after two redirects, the endpoint returned `200` with `ClickCount = 2`, the exact URL and timestamps; an unknown code returned `404`; live Swagger documented the route and response schema.

## Phase 6: Production Concerns

Status: **Complete**

- [x] Add FluentValidation for create requests.
- [x] Execute URL validation explicitly in `UrlsController`.
- [x] Add Serilog structured logging.
- [x] Add structured controller outcome logs without destination URLs.
- [x] Add structured service logs for collision retries, retry exhaustion, resolution, analytics, and visit recording.
- [x] Add global exception handling with Problem Details.
- [x] Add Swagger/OpenAPI metadata and UI.
- [x] Add process liveness and SQL readiness health checks.
- [x] Remove the committed local SQL password and use Windows-authenticated LocalDB by default.
- Validation: invalid URLs returned clear `400` responses from explicit controller validation; Swagger documented all APIs; structured logs captured controller outcomes, request status/duration, trace IDs, and internal exceptions without destination URLs; healthy probes returned `200`; an unavailable SQL database produced liveness `200`, readiness `503`, and a sanitized `500` containing a trace ID.

## Phase 7: Tests

Status: **Complete**

- [x] Test short-code format and collision retries.
- [x] Test URL creation service behavior.
- [x] Test redirect found/not-found behavior.
- [x] Test atomic analytics behavior against real SQL Server LocalDB.
- [x] Test cache hit, miss, write failure, and read failure fallback.
- [x] Test validation and sanitized exception responses.
- [x] Add focused API integration tests with `WebApplicationFactory`.
- Validation: 27 tests passed on native .NET 10 with zero failures or skips; coverage measured 87.16% lines and 71.87% branches; repository tests used disposable LocalDB databases rather than EF InMemory; NuGet reported no known vulnerable direct or transitive packages.

## Phase 8: Delivery

Status: **Complete**

- [x] Add a multi-stage Dockerfile and Compose stack for API, SQL Server, and official Redis.
- [x] Add health-gated startup and an opt-in Compose migration path.
- [x] Add setup and operation instructions to `README.md`.
- [x] Add `docs/prd.md`.
- [x] Add `docs/architecture.md`.
- [x] Add `docs/rules.md`.
- [x] Add `docs/tasks.md`.
- [x] Add `docs/Memory.md`.
- [x] Add final AI usage and traceability report.
- [x] Add final engineering summary, risks, trade-offs, assumptions, and limitations.
- Validation: Compose YAML and references passed static checks; the image built with pinned .NET 10 Noble images; migration-on-startup completed; API, SQL Server, and official Redis containers reported healthy; liveness and SQL readiness returned `200`; final build/tests/audit/model-drift gates passed.

## Assignment Scenarios

- Greenfield: initial URL shortener design and implementation.
- Brownfield: repository-tier refactor and interface co-location after working behavior existed.
- Ambiguous: redirect analytics consistency decision; synchronous updates selected over fire-and-forget or a background queue.