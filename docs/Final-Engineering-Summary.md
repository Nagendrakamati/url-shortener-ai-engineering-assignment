# Final Engineering Summary

## Outcome

The project converts an ambiguous URL-shortener assignment into a runnable ASP.NET Core 10 system with three core APIs, SQL Server persistence, Redis cache-aside reads, aggregate analytics, structured operational behavior, automated tests, and reproducible container delivery artifacts.

## Delivered Artifacts

- `POST /api/urls` URL creation with secure random Base62 codes and bounded collision retries.
- `GET /{shortCode}` redirect with Redis-first lookup and awaited atomic SQL analytics.
- `GET /api/urls/{shortCode}/analytics` SQL-authoritative aggregate analytics.
- Controller -> Service -> Repository responsibility boundaries.
- EF Core migration with case-sensitive unique code index.
- FluentValidation, Serilog, Problem Details, Swagger, liveness, and readiness.
- Multi-stage .NET 10 Dockerfile and health-gated Compose stack with SQL Server and official Redis.
- 27 unit/integration tests with real LocalDB relational coverage.
- Product, architecture, task, AI usage, and setup documentation.

## Key Decisions

| Decision | Rationale | Trade-off |
|---|---|---|
| One layered deployable | Appropriate delivery and operations complexity for 2-3 days | Tiers are logical, not independently deployable |
| SQL Server authority | Required stack and strong EF Core integration | Every redirect performs an analytics write |
| Random eight-character Base62 | Opaque, large keyspace, simple generation | Requires rare collision retry |
| SQL unique index as collision authority | Correct under concurrency | Collision is detected during insert |
| Redis cache-aside destinations only | Improves hot redirect reads without affecting correctness | Cache outage adds bounded fallback latency |
| Synchronous analytics update | Simple, reliable, immediately queryable | Adds SQL latency to redirect |
| Aggregate analytics columns | Minimal schema and API scope | No per-click event history |
| Explicit mapping and validation | Easy to review and debug | Some small manual mapping code |
| Opt-in Compose migration | One-command reviewer setup | Multi-replica production needs a separate migration job |

## Validation Evidence

- `net10.0` solution builds with zero warnings and errors.
- EF Core CLI `10.0.12` discovers the initial migration and reports no pending model changes.
- 27 tests passed with zero failures/skips.
- Coverage: 87.16% lines and 71.87% branches.
- NuGet audit found no known vulnerable direct or transitive packages.
- Live create, redirect, analytics, validation, Swagger, liveness, and readiness checks passed.
- Redis hit, miss, write failure, and read failure fallback were tested.
- Simulated SQL failure produced liveness `200`, readiness `503`, and sanitized `500` Problem Details.
- Logs correlate outcomes and exceptions by trace ID without logging destination URLs.

## Risks And Mitigations

| Risk | Mitigation |
|---|---|
| Random code collision | Case-sensitive unique index and five bounded retries |
| Redis outage | Catch cache failures, log warning, fall back to SQL |
| Stale cache | URLs are immutable in current scope and TTL is bounded |
| Lost analytics | Await atomic SQL update before redirect response |
| SQL outage | Readiness fails and global handler returns sanitized Problem Details |
| Sensitive logging | Structured allowlisted properties; no request bodies/destination URLs |
| Package vulnerabilities | Pinned dependencies and NuGet audit |
| Migration race in scaled deployment | Migration-on-startup limited to local Compose; production uses release job |
| Public abuse/phishing | Explicitly out of scope; add authentication, quotas, and reputation checks before public launch |

## Assumptions

- Assignment-scale traffic and a single deployment region.
- URLs are immutable after creation.
- Aggregate counts are sufficient analytics.
- SQL Server is available for successful redirects because analytics are synchronous.
- Docker Compose is for reviewer/local development, not a production orchestrator.

## Limitations

- No authentication, ownership, custom aliases, expiration, edit, or delete APIs.
- No per-click event history, referrer, device, or geographic analytics.
- No distributed tracing exporter or metrics backend.
- No advanced abuse prevention.

## Recommended Next Steps

1. Move migration execution into a dedicated deployment job for multiple API replicas.
2. Add authentication, ownership, quotas, and abuse controls before public exposure.
3. Add OpenTelemetry metrics/traces and deployment-specific alerts when an observability backend is selected.