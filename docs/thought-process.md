# Thought Process

## Initial Interpretation

The challenge requires a production-minded but intentionally simple task management system. The constraints are explicit: no ORM, no MediatR, no mediator pattern — raw ADO.NET with Npgsql only. This is designed to test architectural discipline and the ability to deliver clean, layered code without relying on framework scaffolding.

## Architectural Reasoning

Clean Architecture was applied strictly:

- **Domain** has zero external dependencies. It holds entities, enums, and domain validation. Business rules live here.
- **Application** depends only on Domain. It contains use cases (one class per operation), DTOs, and repository/service interfaces. No infrastructure concerns leak in.
- **Infrastructure** implements the Application interfaces. It handles PostgreSQL access via raw ADO.NET, JWT generation, and password hashing.
- **API** is a thin shell: it wires DI, handles HTTP concerns, and delegates all logic to use cases. Controllers contain no business logic.

The Use Case pattern was chosen over generic service classes (e.g., `TaskService`) to keep each operation small, independently testable, and clearly named.

## Business Rule Decisions

- **Ownership** is enforced in the Application layer by always passing the authenticated user ID to use cases and from there into repository queries. Repositories never decide ownership — they receive a `userId` filter.
- **404 over 403** for cross-user task access: returning 404 avoids leaking the existence of resources owned by other users (security by obscurity for resource enumeration).
- **BCrypt (BCrypt.Net-Next, work factor 11)** for password hashing: adaptive cost factor, widely used, no separate salt storage needed since BCrypt embeds the salt in the hash output. The `password_salt` column in the schema is always stored as an empty string as a result — the column exists per the spec but is not functionally used.
- **Done is terminal**: once a task reaches Done, no further status changes are allowed and deletion is rejected at the domain level.

## Frontend Decisions

- React + TypeScript + Vite for a fast, modern setup.
- Tailwind CSS for utility-first responsive styling without a component library dependency.
- JWT stored in localStorage for this exercise, with the trade-off documented (httpOnly cookies preferred in production).
- React Context for auth state — Redux is unnecessary at this scale.

## Testing Strategy

A TDD-inspired approach was used:

1. Domain tests were written first to validate business rules (title, due date, status transitions, deletion protection).
2. Use case tests were written next, with mocked repositories, to validate behavior (ownership, auth rejection, duplicate email).
3. Implementation followed to pass the tests.
4. Minimal repository integration tests run against a real PostgreSQL instance.

Full strict TDD was not claimed — only core rules and use cases were developed test-first.

## Trade-offs

- No refresh tokens: JWT expiry is short-lived; re-login is required.
- No pagination: acceptable for the exercise scope.
- No rate limiting or advanced observability.
- Minimal integration tests: focused on the most critical data access paths.
- localStorage for JWT: acknowledged and documented.

## What Was Intentionally Not Implemented

- Refresh token rotation (would require a token store and rotation logic).
- Pagination, filtering, and sorting (YAGNI for this exercise).
- Role-based authorization beyond user ownership.
- Advanced logging and distributed tracing.
