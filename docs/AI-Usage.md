# AI Usage And Traceability

## Operating Principle

AI assisted analysis, implementation, debugging, testing, and documentation. The engineer retained ownership of requirements, architecture, security, correctness, scope, and final acceptance. Generated output was not accepted solely because it compiled; each phase used executable checks and reviewable evidence.

No production secrets, customer data, or proprietary source repositories were provided to external tools. The assignment statement contained a Schwab Internal classification, so the implementation documentation paraphrases requirements and does not reproduce unnecessary internal content.

## Prompt Discipline

Tasks were given to AI with:

- Intent and phase boundary.
- Required stack and time constraint.
- Existing architecture and affected files.
- Acceptance criteria and expected status codes.
- Security, privacy, and complexity constraints.
- A request for focused validation after each change.

## Traceability

| Scenario | AI contribution | Engineer review or change | Quality gate |
|---|---|---|---|
| Greenfield foundation | Proposed a small ASP.NET API, EF model, migration, and random Base62 generation | Kept one deployable and rejected Bitly-scale infrastructure | Build, migration discovery, model-drift check, LocalDB smoke test |
| Ambiguous redirect analytics | Compared synchronous, fire-and-forget, and background queue options | Selected awaited atomic SQL update for reliable assignment-scale behavior | Live `302`, SQL `ClickCount`, UTC timestamp, concurrency-safe update SQL |
| Brownfield architecture | Identified EF details leaking from service | Introduced a domain-specific repository; rejected a generic repository | Build plus live create/redirect regression test |
| Redis enhancement | Proposed SQL-authoritative cache-aside behavior | Limited Redis to destinations; excluded analytics and unknown-code caching | Cache hit/miss/failure tests and live Redis-protocol inspection |
| Production concerns | Drafted validation, Serilog, Problem Details, Swagger, and health-check integration | Explicit controller validation; SQL-only readiness; privacy-conscious logs | Invalid-input matrix, `500` sanitization, `200/503` health checks |
| Test generation | Generated focused unit, repository, and API tests | Used real LocalDB instead of EF InMemory; patched vulnerable test dependency | 27/27 tests, coverage, NuGet vulnerability audit |
| Runtime migration | Identified TestHost failure from net8 roll-forward onto .NET 10 | Retargeted solution and dependencies to native .NET 10 | Zero-warning build, EF model check, complete test suite, runtime smoke test |
| Delivery | Generated Docker and reviewer documentation | Kept startup migration opt-in, corrected .NET image tags, and documented separate production migration | Full Compose build; healthy API, SQL Server, and Redis containers; `200` liveness/readiness |

## Generated, Edited, Rejected

### Generated And Accepted After Review

- Initial solution and EF Core schema.
- Controller, service, repository, cache, analytics, validation, logging, and health-check implementations.
- Unit and integration test matrix.
- Docker and documentation artifacts.

### Edited During Review

- Simplified the original architecture from queues/workers to one layered deployable.
- Moved EF Core and SQL exceptions from service to repository.
- Split contracts and interfaces into review-friendly folders.
- Replaced automatic validation with explicit controller validation.
- Removed committed development credentials.
- Updated packages after NuGet reported a high-severity advisory.
- Retargeted from .NET 8 to .NET 10 to match the approved current implementation and runtime.

### Rejected With Rationale

- Microservices, CQRS, MediatR, message brokers, and outbox: disproportionate for 2-3 days.
- Fire-and-forget analytics: can lose updates and misuse request-scoped services.
- Background analytics queue: adds shutdown, retry, and durability concerns.
- Generic repository: duplicates EF Core abstractions without domain value.
- AutoMapper: two small mappings are clearer and safer as explicit constructors.
- EF InMemory tests: do not validate SQL indexes, collations, migrations, or atomic updates.
- Sequential short codes: predictable and enumerable.
- Negative caching and stampede locks: unnecessary assignment complexity.

## Human Sign-Off Gates

The engineer must explicitly review before merge:

- Database schema and migrations.
- Authentication/authorization changes if later introduced.
- Secret and logging configuration.
- Package vulnerability findings.
- Cache consistency changes.
- Docker image and infrastructure changes.
- Any modification that changes redirect or analytics semantics.

## Known AI Risks And Controls

| Risk | Control |
|---|---|
| Hallucinated APIs or package versions | Restore/build against pinned versions and consult tool output |
| Plausible but incorrect failure handling | Simulate SQL and Redis failures and inspect responses/logs |
| Over-engineering | Enforce phase scope and document rejected alternatives |
| Weak tests that mirror implementation | Test public behavior and real SQL semantics |
| Secret exposure | Search the workspace, use `.env` only locally, and avoid logging destination URLs |
| Silent regressions | Run full build/test suite after each cross-cutting change |

## Ownership Statement

AI accelerated implementation and review preparation. The engineer is responsible for validating every artifact, understanding each dependency and trade-off, and deciding whether the solution is acceptable for submission and deployment.