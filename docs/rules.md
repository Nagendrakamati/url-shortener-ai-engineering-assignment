# Engineering Rules

## Architecture

- Preserve Controller -> Service -> Repository dependency flow.
- Controllers handle HTTP concerns only.
- Services own business rules and orchestration.
- Repositories own EF Core, SQL Server, and Redis access.
- Depend on interfaces at layer boundaries.
- Do not introduce a generic repository, CQRS, MediatR, or microservices without a documented need.

## C# and .NET

- Target .NET 10 and use nullable reference types.
- Use asynchronous APIs for I/O and pass `CancellationToken` through every layer.
- Use UTC for persisted timestamps.
- Prefer immutable request and response records.
- Use descriptive names and avoid unexplained one-letter identifiers.
- Keep methods focused and avoid speculative abstractions.
- Do not expose EF entities as public API contracts.

## Persistence

- SQL Server is the source of truth.
- Enforce correctness with database constraints, not pre-checks alone.
- Keep short-code collision handling concurrency-safe through the unique index.
- Use no-tracking reads when entities will not be modified.
- Use atomic SQL updates for counters.
- Add an EF migration for every schema change and verify snapshot consistency.

## Caching

- Use Redis only as a cache and optional rate-limiting store.
- Follow cache-aside behavior: Redis -> SQL fallback -> populate Redis.
- Never require Redis for data correctness.
- Use bounded TTLs and log cache failures without exposing sensitive values.

## API

- Use explicit request and response contracts under `Contracts/Requests` and `Contracts/Responses`.
- Return `201` for creation, `302` for redirects, `400` for validation failures, `404` for missing resources, and Problem Details for errors.
- Accept only absolute HTTP and HTTPS destination URLs.
- Keep OpenAPI metadata synchronized with actual responses.

## Security and Logging

- Never commit secrets or production credentials.
- Do not log passwords, tokens, raw IP addresses, or complete sensitive query strings.
- Validate input length, scheme, and format before persistence.
- Use parameterized EF Core queries and bounded request sizes.
- Return trace identifiers rather than internal exception details.

## Testing and Quality Gates

- Build with zero warnings and errors before completing a task.
- Add focused tests for business rules, repository behavior, endpoint contracts, and failure paths.
- Use SQL Server-compatible integration testing for persistence claims; do not substitute EF InMemory for relational behavior.
- Verify migrations, create/redirect behavior, analytics, Redis fallback, and exception responses.
- Do not mark a task complete without recording validation evidence.

## AI-Assisted Engineering

- State intent, constraints, acceptance criteria, and affected technical context in prompts.
- Review every generated change before accepting it.
- Record generated, edited, and rejected decisions with rationale in project documentation.
- Never send secrets, credentials, customer data, or Schwab internal information to unapproved AI systems.
- Require engineer sign-off for schema, security, dependency, and architecture changes.
- The engineer remains accountable for correctness, maintainability, security, and production readiness.

## Documentation

- Keep `docs/prd.md`, `docs/architecture.md`, `docs/tasks.md`, and `docs/Memory.md` current as decisions change.
- Record assumptions and limitations explicitly.
- UI guidance belongs in `design.md` only if UI scope is introduced.