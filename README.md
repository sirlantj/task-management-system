# Task Management System

## Overview

A full-stack task management web application. It demonstrates Clean Architecture, raw ADO.NET data access, JWT authentication, and a React + TypeScript frontend without relying on ORMs, mediator frameworks, or generated scaffolding.

---

## User Story

> "As a registered user, I want to securely manage my own tasks so that I can organize my work, track progress, and keep my personal task list private."

---

## Architecture

The backend follows Clean Architecture with four layers:

```
┌──────────────────────────────────────────────┐
│                    API                       │
│   Controllers · Middleware · DI · Swagger    │
└────────────────────┬─────────────────────────┘
                     │ depends on
┌────────────────────▼─────────────────────────┐
│               Application                    │
│      Use Cases · DTOs · Interfaces           │
└────────────┬──────────────────────────────────┘
             │ depends on           ▲ implements
┌────────────▼──────────┐  ┌───────┴──────────────────┐
│        Domain         │  │     Infrastructure        │
│  Entities · Enums     │  │  Repositories · JWT       │
│  Domain Rules         │  │  Password Hashing         │
└───────────────────────┘  └──────────────────────────┘
```

**Domain** has zero external dependencies. It contains entities, enums, and all domain validation. Business rules live here.

**Application** depends only on Domain. It contains one use case class per operation, DTOs, and repository/service interfaces. No infrastructure concerns leak in.

**Infrastructure** implements the Application interfaces. It handles PostgreSQL access via raw ADO.NET (Npgsql), JWT generation, and password hashing. It depends on both Application (interfaces) and Domain (entities).

**API** is a thin shell: it wires DI, handles HTTP concerns, and delegates all logic to use cases via constructor-injected use case classes. Controllers contain no business logic.

### Why no EF, Dapper, or MediatR

- **Entity Framework**: introduces generated migrations and an abstraction layer over SQL that obscures intent. Raw ADO.NET keeps data access explicit and reviewable.
- **Dapper**: a lighter option, but still ORM-adjacent. Excluded by constraint and by the goal of making every query deliberate.
- **MediatR**: adds indirection (request → handler pipeline) with no meaningful benefit at this scale. Use cases are invoked directly from controllers.

---

## Tech Stack

**Backend**

- .NET 8 / C#
- ASP.NET Core Web API (MVC Controllers)
- Npgsql (raw ADO.NET, no ORM)
- BCrypt.Net-Next (password hashing)
- Microsoft.AspNetCore.Authentication.JwtBearer
- Swashbuckle (Swagger/OpenAPI)
- xUnit (tests)
- Moq (mocking in use case tests)
- Microsoft.AspNetCore.Mvc.Testing (API integration tests)

**Frontend**

- React 19
- TypeScript 6
- Vite 8
- Tailwind CSS 3
- react-router-dom v7

**Database**

- PostgreSQL 16 (via Docker)

**Tooling**

- Docker / Docker Compose
- .NET 8 SDK
- Node.js 20+

---

## Project Structure

```
task-management-system/
├── backend/
│   ├── TaskManagementSystem.Api/           # Controllers, middleware, DI wiring, Swagger
│   ├── TaskManagementSystem.Application/   # Use cases, DTOs, repository/service interfaces
│   ├── TaskManagementSystem.Domain/        # Entities, enums, domain validation, domain exceptions
│   ├── TaskManagementSystem.Infrastructure/# ADO.NET repos, JWT service, password hashing
│   └── TaskManagementSystem.Tests/
│       ├── Domain/                         # Domain rule unit tests
│       ├── Application/                    # Use case tests (mocked dependencies)
│       ├── Integration/                    # Repository integration tests (live DB)
│       └── Api/                            # Controller integration tests (WebApplicationFactory)
├── frontend/
│   └── src/
│       ├── api/        # fetch wrapper with error handling and auth header injection
│       ├── auth/       # AuthContext (React Context) and ProtectedRoute
│       ├── components/ # Layout, TaskCard, TaskForm
│       ├── pages/      # LoginPage, RegisterPage, TaskListPage, CreateTaskPage, EditTaskPage
│       └── types/      # TypeScript type definitions (Task, TaskStatus, payloads)
├── docs/
│   ├── user-story.md
│   ├── thought-process.md
│   └── ai-usage.md
├── docker-compose.yml
├── init.sql            # PostgreSQL schema + seed data
├── .env.example        # Environment variable reference (no secrets)
└── README.md
```

---

## Async Design

All I/O-bound backend operations are async end-to-end:

```
Controller action (async Task<IActionResult>)
  → UseCase.ExecuteAsync(CancellationToken)
    → Repository.XxxAsync(CancellationToken)
      → Npgsql async APIs (OpenAsync, ExecuteReaderAsync, ReadAsync, ...)
```

`CancellationToken` is accepted by controller actions (ASP.NET Core injects it automatically from `HttpContext.RequestAborted`) and propagated to use cases and repositories. This allows in-flight database operations to be cancelled when a client disconnects.

Domain entity methods (validation, status transitions) remain synchronous — they are CPU-only operations with no I/O. Password hashing (BCrypt) is also synchronous because it is CPU-bound. JWT generation is synchronous for the same reason.

Blocking constructs (`.Result`, `.Wait()`, `GetAwaiter().GetResult()`) are not used anywhere in the codebase.

---

## How to Run

### Prerequisites

- Docker and Docker Compose
- .NET 8 SDK
- Node.js 20+

### 1. Configure environment

```bash
cp .env.example .env
```

Edit `.env` and set a strong value for `JWT_SECRET` (at least 32 characters). The default values in `.env.example` and `appsettings.json` are for local development only — they must not be used in any other environment.

### 2. Start the database

```bash
docker-compose up -d
```

PostgreSQL will start on port **5433** and run `init.sql` automatically on first launch, creating the schema and seeding the demo user.

### 3. Run the backend

```bash
cd backend
dotnet restore
dotnet run --project TaskManagementSystem.Api
```

The API listens on `http://localhost:5030`. Swagger UI is available at `http://localhost:5030/swagger` in the Development environment.

### 4. Run the frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend runs on `http://localhost:5173`. All `/api/*` requests are proxied to `http://localhost:5030` via the Vite dev server — no CORS configuration needed in the browser during development.

### 5. Run the tests

```bash
cd backend
dotnet test
```

**Note:** Domain unit tests and use case tests (with mocked dependencies) run without any external services. Repository integration tests and API integration tests require the PostgreSQL container to be running with the seed data from `init.sql`.

---

## Demo Credentials

```
Email:    demo@taskmanagement.local
Password: Demo@12345
```

---

## API Endpoints

| Method   | Path                 | Auth     | Description                                                 |
| -------- | -------------------- | -------- | ----------------------------------------------------------- |
| `POST`   | `/api/auth/register` | Public   | Register a new user. Returns JWT.                           |
| `POST`   | `/api/auth/login`    | Public   | Authenticate and receive a JWT.                             |
| `GET`    | `/api/auth/me`       | Required | Returns the authenticated user's profile.                   |
| `GET`    | `/api/tasks`         | Required | List all tasks for the authenticated user.                  |
| `GET`    | `/api/tasks/{id}`    | Required | Get a single task by ID (scoped to the authenticated user). |
| `POST`   | `/api/tasks`         | Required | Create a new task.                                          |
| `PUT`    | `/api/tasks/{id}`    | Required | Update a task's title, description, due date, or status.    |
| `DELETE` | `/api/tasks/{id}`    | Required | Delete a task (rejected if status is Done).                 |

**Error response shape:**

```json
{
  "error": "Human-readable message",
  "details": null
}
```

---

## Design Decisions

**Use Case pattern over service classes**
Each operation (CreateTask, DeleteTask, Login, ...) is its own class with a single `ExecuteAsync` method. This keeps units small, independently testable, and clearly named. A `TaskService` god-class would grow unbounded.

**Raw ADO.NET with Npgsql**
All database access uses parameterized `NpgsqlCommand` queries written explicitly. No query builder, no mapper, no ORM abstraction. Row mapping is manual and visible. This makes SQL injection impossible (no string interpolation) and every query intentional.

**BCrypt for password hashing**
BCrypt stores the salt as part of the hash string itself. The password_salt column is retained because the schema explicitly includes it, but for BCrypt-based hashes the separate salt value is not required.

**JWT authentication**
Tokens are signed with HMAC-SHA256. `MapInboundClaims = false` prevents ASP.NET Core from remapping the `sub` claim, ensuring the user ID is read consistently in controllers using `JwtRegisteredClaimNames.Sub`. `ClockSkew = TimeSpan.Zero` enforces strict token expiry.

**404 over 403 for cross-user access**
When a user requests a task that belongs to another user, the API returns 404 (Not Found) rather than 403 (Forbidden). This avoids revealing whether a resource exists at all, reducing the risk of resource enumeration.

**JWT in localStorage**
Stored in localStorage for simplicity in this exercise. The trade-off is XSS exposure: any injected script can read the token. In production, prefer `httpOnly` secure cookies (not accessible from JavaScript), refresh-token rotation, or a Backend-for-Frontend (BFF) pattern that keeps tokens server-side.

**React Context for auth state**
Redux is unnecessary at this scale. Auth state (user object, loading flag, login/logout functions) is managed in a single `AuthContext`. The `isLoading` flag prevents the `ProtectedRoute` from redirecting to `/login` before the token validation (`GET /api/auth/me`) has completed on mount.

**Vite dev proxy**
The Vite dev server proxies all `/api/*` requests to `http://localhost:5030`. This avoids CORS issues during development without modifying browser or server CORS headers. The backend CORS policy (`http://localhost:5173`) is still configured for direct API access or production builds.

---

## Trade-offs

- **No refresh tokens.** JWT tokens expire after 60 minutes and require re-login. A production system would implement silent refresh with short-lived access tokens and long-lived refresh tokens stored in `httpOnly` cookies.
- **No pagination.** `GET /api/tasks` returns all tasks for a user. Acceptable at exercise scale; not acceptable in production.
- **No filtering or sorting.** Tasks are returned ordered by `created_at DESC`.
- **No rate limiting.** Login and registration endpoints are not rate-limited.
- **No observability.** No structured logging, distributed tracing, or metrics beyond the default ASP.NET Core request logging.
- **Repository integration tests require a live database.** There is no test database abstraction or container-per-test setup (e.g., Testcontainers). The tests use a hardcoded connection string pointing to the Docker Compose instance.
- **API integration tests require the seeded demo user.** The two integration-tagged controller tests authenticate as `demo@taskmanagement.local` and depend on the seed data from `init.sql`.
- **`password_salt` column is always empty.** BCrypt embeds the salt in the hash. The column exists in the schema per the spec but holds no data in practice.

---

## TDD Approach

A TDD-inspired workflow was applied to core business rules and use cases:

1. **Domain tests first.** Tests for title validation, due date validation, status transition rules, and deletion protection were written before the corresponding `TaskItem` methods.
2. **Use case tests second.** Tests for ownership enforcement, duplicate email rejection, invalid credentials, and completed-task deletion were written with mocked repositories before implementing use case bodies.
3. **Implementation followed** to make the tests pass.
4. **Repository and API integration tests** were added after infrastructure was implemented to validate the full stack.

Full strict TDD was not applied across the entire codebase. Infrastructure plumbing (DI wiring, Swagger configuration, Vite setup) was written directly without preceding tests. The domain layer and Application use cases followed the test-first discipline most closely.

---

## GenAI Usage

See [docs/ai-usage.md](docs/ai-usage.md) for a detailed account.

In summary: Claude was used as the development driver throughout the project, receiving the full specification in `instructions.md` and implementing it phase by phase. Each phase was reviewed before committing. Factual errors in generated output (incorrect password hashing algorithm references, TypeScript strict-mode incompatibilities, status transition bugs introduced during integration) were caught and corrected manually.

---

## Known Limitations

- The `password_salt` DB column is always empty — BCrypt was used instead of a hash+salt scheme where the salt is stored separately.
- Repository integration tests and API integration tests are not isolated: they write test data to the shared development database and do not clean up after themselves consistently.
- No input length validation beyond domain rules (e.g., no max-length enforcement on title at the API layer).
- No email format validation in `RegisterUserUseCase` beyond what the database uniqueness constraint enforces.
- Swagger is only enabled in the Development environment.

---

## Future Improvements

- Refresh token rotation with `httpOnly` secure cookies or a BFF pattern
- Pagination, filtering, and sorting on `GET /api/tasks`
- Email format validation and password strength rules
- Rate limiting on auth endpoints
- Structured logging (e.g., Serilog) with correlation IDs
- Distributed tracing and health check endpoints
- Testcontainers for isolated, reproducible integration tests
- Frontend unit/component tests (e.g., Vitest + React Testing Library)
- CI pipeline (build, test, lint on every push)
- Input length enforcement at the API model-binding layer
