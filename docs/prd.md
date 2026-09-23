# Product Requirements Document

## Product

URL Shortener API for the Charles Schwab AI-Proficient Software Engineer assignment.

## Objective

Build a reviewable, production-conscious prototype in 2-3 days that demonstrates sound .NET engineering and disciplined AI-assisted execution. The engineer owns every decision and validates all AI-assisted output.

## Users

- API consumers creating short URLs.
- Visitors following short URLs.
- Reviewers inspecting aggregate URL analytics.
- Engineers reviewing architecture, tests, validation, and AI-use traceability.

## Functional Requirements

### Create a short URL

- Accept an absolute HTTP or HTTPS URL.
- Generate a cryptographically random, eight-character Base62 code.
- Store the code and original URL in SQL Server.
- Retry code generation when SQL Server reports a unique-key collision.
- Return `201 Created`, the generated code, short URL, original URL, and creation time.

### Redirect a short URL

- Resolve a known code to its original URL.
- Return `302 Found` for a known code.
- Return `404 Not Found` for an unknown code.
- Await an atomic SQL update of `ClickCount` and `LastAccessedAtUtc` before returning the redirect.
- Use Redis as a cache-aside optimization once Phase 4 is implemented; SQL Server remains authoritative.

### View analytics

- Return the short code, original URL, creation time, click count, and last-access time.
- Read aggregate analytics from SQL Server.
- Return `404 Not Found` for an unknown code.

## Non-Functional Requirements

- Target ASP.NET Core 10 and EF Core 10.
- Use Controller -> Service -> Repository dependency flow.
- Keep SQL Server as the source of truth.
- Treat Redis failure as a performance degradation, not a correctness failure.
- Use structured logs without exposing secrets or sensitive URL query values.
- Return consistent Problem Details responses for unexpected failures.
- Validate requests before business logic executes.
- Provide reproducible local startup through Docker in the final phase.
- Cover core business behavior with focused xUnit tests.

## Constraints and Assumptions

- Delivery time is 2-3 days.
- This is an API-only assignment; no UI is required.
- Authentication, user accounts, custom aliases, URL expiration, and editing are not required.
- Analytics are aggregate-only; individual click events are not stored.
- Single-region and moderate assignment-scale traffic are assumed.

## Acceptance Criteria

- A reviewer can start the dependencies and API using documented commands.
- Create, redirect, and analytics APIs work end to end.
- Duplicate short codes cannot be persisted under concurrent requests.
- A redirect increments analytics exactly once for each successful application request.
- Redis cache misses and outages fall back to SQL Server.
- Build and automated tests pass without warnings or errors.
- Swagger documents the public API.
- Architecture, risks, assumptions, AI usage, and validation evidence are documented.

## Out of Scope

- Web or mobile UI; therefore `design.md` is intentionally omitted.
- Microservices, CQRS, message brokers, background analytics workers, and event sourcing.
- Multi-region deployment, Kubernetes, edge caching, and read replicas.
- Per-click geographic, device, or referrer analytics.
- Enterprise phishing detection and domain-reputation integrations.

## Open Decisions

- Whether Redis-backed rate limiting should be added before any public deployment.
- Whether authentication and ownership are required in a future product phase.