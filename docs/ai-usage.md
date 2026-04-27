# AI Usage

## Prompt Used

The following prompt was used to scaffold and guide the implementation:

> "You are a Principal Software Engineer specializing in .NET 8, Clean Architecture, ADO.NET, PostgreSQL, React, testing, and scalable backend systems. Your task is to generate a review-ready, production-minded, but intentionally simple Task Management web application..."
> (full prompt in instructions.md)

## Representative Sample of AI-Generated Output

The AI generated the initial folder structure, project references, NuGet package selection, domain entity shapes, use case skeletons, ADO.NET repository patterns, JWT service implementation, and React component scaffolding.

## How Generated Code Was Validated

- Each layer was reviewed against Clean Architecture rules (no upward dependencies, no infrastructure concerns in Domain/Application).
- Domain validation was verified by running domain unit tests before writing any infrastructure or API code.
- Use case logic was verified by running use case tests with mocked repositories.
- API responses were manually tested via Swagger UI after wiring the full stack.

## What Was Manually Corrected

- Ensured repository queries used parameterized `NpgsqlCommand` parameters, not string interpolation.
- Verified that `DbConnectionFactory` opens and disposes connections per operation (no long-lived connections).
- Confirmed JWT claims extraction in controllers was consistent with token generation in infrastructure (`MapInboundClaims = false` required to prevent claim name remapping).
- Adjusted error response shapes to match the documented `{ "error": "...", "details": "..." }` format.
- Fixed a TypeScript strict-mode incompatibility: `erasableSyntaxOnly: true` does not allow constructor parameter properties. `ApiError.status` was moved to an explicit class field declaration.
- Fixed a status transition bug introduced during integration review: `EditTaskPage` always sent the current `status` in the PUT payload, which caused a `Done → Done` transition error. Fixed to only send `status` when it differs from the current task status.
- Corrected documentation that incorrectly referenced PBKDF2 as the password hashing algorithm. The implementation uses BCrypt (BCrypt.Net-Next, work factor 11), which embeds the salt in the hash output. The `password_salt` column is stored as an empty string as a result.

## How Edge Cases Were Handled

- Past due date validation was implemented with UTC comparison to avoid timezone-related false positives.
- Status transition logic was isolated in the domain entity and throws a typed domain exception for invalid transitions.
- Duplicate email on registration returns 409 Conflict rather than a generic 400.

## How Authentication and Validations Were Reviewed

- All task endpoints were confirmed to require a valid JWT (401 on missing/invalid token).
- Ownership was verified: a test was added to confirm a user cannot retrieve or modify another user's task.
- Password is never stored in plain text; only hash and salt are persisted.

## Security Concerns Checked

- SQL injection: all queries use parameterized `NpgsqlCommand` parameters — no string interpolation.
- JWT secret is read from configuration/environment variables, never hardcoded in source.
- localStorage JWT trade-off is documented with a recommendation for httpOnly cookies in production.
- Password hashing uses BCrypt (work factor 11). The hash includes the salt — no plain-text password is ever stored.
- All task endpoints confirmed to scope queries by authenticated user ID — no cross-user data leakage possible at the repository layer.

## Why EF, Dapper, and MediatR Were Avoided

- **Entity Framework**: introduces magic, migrations, and a leaky abstraction over SQL. For this exercise, explicit ADO.NET makes data access intent clear and reviewable.
- **Dapper**: while lighter than EF, it is still an ORM-adjacent library. Raw ADO.NET was required by the constraints and forces deliberate query authoring.
- **MediatR**: adds indirection and a dependency for no meaningful benefit at this scale. Use cases are invoked directly from controllers.

## Final Code Review

- All files were reviewed for Clean Architecture compliance before submission.
- No business logic was found in controllers or repositories.
- All tests pass.
- No compiler warnings remain.
