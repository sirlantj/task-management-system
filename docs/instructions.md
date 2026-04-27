You are a Principal Software Engineer specializing in .NET 8, Clean Architecture, ADO.NET, PostgreSQL, React, testing, and scalable backend systems.

Your task is to generate a review-ready, production-minded, but intentionally simple Task Management web application for a technical interview exercise.

The solution must include:

1. ASP.NET Core Web API backend
2. PostgreSQL data store
3. React + TypeScript frontend
4. Clean Architecture
5. TDD-inspired tests focused on meaningful business rules and use case behavior
6. README and documentation explaining the user story, thought process, AI usage, trade-offs, and setup instructions

The final project should be suitable for senior-level code review.

==================================================
HARD CONSTRAINTS
==================================================

Do not violate these constraints:

- Use .NET 8 and C#
- Use ASP.NET Core Web API with MVC Controllers
- DO NOT use Entity Framework
- DO NOT use Dapper
- DO NOT use any ORM
- DO NOT use MediatR
- DO NOT use mediator libraries
- DO NOT use CQRS frameworks
- Use raw ADO.NET with Npgsql for PostgreSQL access
- Use PostgreSQL as the primary database
- Follow Clean Architecture strictly
- Apply SOLID principles
- Follow Clean Code practices
- Enforce DRY
- Apply KISS
- Respect YAGNI and avoid unnecessary abstractions
- Code must be testable, readable, and maintainable
- Avoid overengineering
- Prefer clarity over cleverness

==================================================
EXPECTED REPOSITORY STRUCTURE
==================================================

Create a single monorepo structured like this:

task-management-system/
├── backend/
│ ├── TaskManagementSystem.Api/
│ ├── TaskManagementSystem.Application/
│ ├── TaskManagementSystem.Domain/
│ ├── TaskManagementSystem.Infrastructure/
│ └── TaskManagementSystem.Tests/
├── frontend/
│ ├── src/
│ ├── package.json
│ ├── vite.config.ts
│ └── README.md
├── docs/
│ ├── user-story.md
│ ├── thought-process.md
│ └── ai-usage.md
├── docker-compose.yml
├── init.sql
├── .env.example
├── README.md
└── .gitignore

==================================================
USER STORY
==================================================

Use the following informal user story to drive the implementation:

"As a registered user, I want to securely manage my own tasks so that I can organize my work, track progress, and keep my personal task list private."

Include this user story in:

- README.md
- docs/user-story.md

==================================================
DATABASE REQUIREMENTS
==================================================

Use PostgreSQL via Docker Compose.

Provide:

- docker-compose.yml with a PostgreSQL service
- environment variables via .env
- .env.example with safe placeholder values
- database initialization script: init.sql
- users table
- tasks table
- seed data for demo purposes

Use Npgsql for database connectivity.

Implement a DbConnectionFactory using ADO.NET best practices.

Use UTC consistently for all date/time values.

Database tables:

users:

- id UUID primary key
- name VARCHAR required
- email VARCHAR unique required
- password_hash TEXT required
- password_salt TEXT required
- created_at TIMESTAMPTZ required

tasks:

- id UUID primary key
- title VARCHAR required
- description TEXT nullable
- status VARCHAR required
- due_date TIMESTAMPTZ nullable
- user_id UUID required foreign key to users(id)
- created_at TIMESTAMPTZ required
- updated_at TIMESTAMPTZ nullable

Seed demo credentials:

- Email: demo@taskmanagement.local
- Password: Demo@12345

Store only password hash and password salt in the database.
Never store plain-text passwords.

==================================================
ASYNC REQUIREMENTS
==================================================

Use async/await consistently across the backend request flow.

All I/O-bound operations must be asynchronous.

Required async flow:

Controller action
-> UseCase ExecuteAsync method
-> Repository async method
-> Npgsql async database call

Rules:

- Use async Task<T> or async Task for all use case execution methods.
- Use the suffix Async for asynchronous methods.
- Use CancellationToken parameters where reasonable, especially in:
  - controller actions
  - use case ExecuteAsync methods
  - repository methods
  - database operations
- Pass CancellationToken from controller actions to use cases and from use cases to repositories.
- Use Npgsql async APIs:
  - OpenAsync
  - ExecuteReaderAsync
  - ExecuteNonQueryAsync
  - ExecuteScalarAsync
  - ReadAsync
- Do not use synchronous database calls such as:
  - Open
  - ExecuteReader
  - ExecuteNonQuery
  - ExecuteScalar
  - Read
- Avoid blocking async code.
- Do not use:
  - .Result
  - .Wait()
  - GetAwaiter().GetResult()
- Keep CPU-only domain methods synchronous.
- Domain entity methods such as validation and status transitions do not need to be async.
- Password hashing can be synchronous if using CPU-bound built-in cryptographic APIs.
- JWT generation can be synchronous because it is CPU-bound and does not perform I/O.

==================================================
DOMAIN MODEL
==================================================

User:

- Id
- Name
- Email
- PasswordHash
- PasswordSalt
- CreatedAt

TaskItem:

- Id
- Title
- Description
- Status
- DueDate
- UserId
- CreatedAt
- UpdatedAt

TaskStatus enum:

- Pending
- InProgress
- Done

Domain and business rules:

- Task title is required and cannot be empty or whitespace.
- DueDate cannot be in the past when creating a task or changing the due date.
- Existing overdue tasks can still be read.
- Users can only access their own tasks.
- Completed tasks with status Done cannot be deleted.
- Valid status transitions:
  - Pending -> InProgress
  - Pending -> Done
  - InProgress -> Done
  - Done cannot transition back to Pending or InProgress.

Important:

- Keep domain validation inside Domain/Application layers.
- Do not put business rules inside controllers.
- Do not put business rules inside repositories.
- Ownership rules must be enforced in the Application layer by always passing the authenticated user id to use cases and repository queries.
- Completed task deletion must be rejected by DeleteTaskUseCase using a domain rule/method on TaskItem.

==================================================
ARCHITECTURE
==================================================

Follow Clean Architecture strictly.

Domain:

- Entities
- Enums
- Domain validation
- Domain exceptions if useful
- No external dependencies

Application:

- Use cases, not generic services
- DTOs
- Interfaces for repositories and services
- Application-specific exceptions if useful
- Depends only on Domain
- All use cases must expose ExecuteAsync methods
- Use cases that call repositories must accept and pass CancellationToken

Infrastructure:

- ADO.NET repositories with Npgsql
- JWT token generation
- Password hashing
- DbConnectionFactory
- Implements Application interfaces
- All repository implementations must be asynchronous
- All database access must use Npgsql async APIs

API:

- Thin controllers
- Dependency injection configuration
- JWT authentication and authorization
- Global exception handling middleware
- CORS configuration for local frontend development
- Swagger/OpenAPI with JWT bearer support
- No business logic
- Controller actions that call use cases must be asynchronous

Frontend:

- React + TypeScript + Vite
- Tailwind CSS for styling
- API client using fetch or axios
- JWT stored in localStorage for simplicity in this technical exercise
- Document the localStorage security trade-off in the README
- Mention that production systems should prefer httpOnly secure cookies, refresh-token rotation, or a BFF approach
- Protected routes
- Task CRUD UI
- Responsive and user-friendly layout
- No browser console warnings

==================================================
APPLICATION LAYER USE CASES
==================================================

Use the Use Case pattern.

Do not create generic service classes like TaskService or UserService to hold all business logic.

All use cases must expose an ExecuteAsync method.

Each ExecuteAsync method should accept a CancellationToken where reasonable.

Authentication use cases:

- RegisterUserUseCase.ExecuteAsync(...)
- LoginUseCase.ExecuteAsync(...)

Task use cases:

- CreateTaskUseCase.ExecuteAsync(...)
- GetTaskByIdUseCase.ExecuteAsync(...)
- GetTasksUseCase.ExecuteAsync(...)
- UpdateTaskUseCase.ExecuteAsync(...)
- DeleteTaskUseCase.ExecuteAsync(...)

Use DTOs for input and output.

Required DTOs:

- RegisterUserRequest
- LoginRequest
- AuthResponse
- CreateTaskRequest
- UpdateTaskRequest
- TaskResponse

Use clear result/error handling.
Keep it simple and avoid overengineering.

==================================================
AUTHENTICATION AND AUTHORIZATION
==================================================

Implement JWT authentication.

Public endpoints:

- POST /api/auth/register
- POST /api/auth/login

Protected endpoints:

- GET /api/auth/me
- GET /api/tasks
- GET /api/tasks/{id}
- POST /api/tasks
- PUT /api/tasks/{id}
- DELETE /api/tasks/{id}

Use claims to identify the authenticated user.

Task endpoints must always scope data by authenticated user id.

A user must never be able to access, update, or delete another user's tasks.

For unauthorized access to another user's task, return either:

- 404 Not Found to avoid revealing resource existence, or
- 403 Forbidden if the resource ownership check is explicit

Choose one approach and document the decision in the README.

Implement secure password storage using hash + salt with built-in .NET cryptographic APIs.

Do not use plain-text passwords.

==================================================
INFRASTRUCTURE REQUIREMENTS
==================================================

Repositories:

- UserRepository
- TaskRepository

Use raw ADO.NET with Npgsql.

Repository interfaces and implementations must be asynchronous.

Repository method examples:

- IUserRepository.GetByEmailAsync(...)
- IUserRepository.GetByIdAsync(...)
- IUserRepository.CreateAsync(...)
- ITaskRepository.CreateAsync(...)
- ITaskRepository.GetByIdAsync(...)
- ITaskRepository.GetByUserIdAsync(...)
- ITaskRepository.UpdateAsync(...)
- ITaskRepository.DeleteAsync(...)

Repository methods must accept CancellationToken where reasonable.

Repository requirements:

- Use parameterized queries only
- Do not use string interpolation for SQL values
- Use proper connection lifecycle
- Use async/await
- Use cancellation tokens where reasonable
- Use Npgsql async APIs only for database operations
- Explicitly map database rows to domain/application objects
- Do not leak infrastructure concerns into Application or Domain
- Do not put business logic inside repositories
- Do not use Entity Framework
- Do not use Dapper
- Do not use ORM-like abstractions

DbConnectionFactory requirements:

- Centralize PostgreSQL connection creation
- Read connection string from configuration/environment variables
- Return NpgsqlConnection
- Do not keep long-lived open connections
- Connections should be opened with OpenAsync by repository methods

==================================================
API REQUIREMENTS
==================================================

Use ASP.NET Core Web API with MVC Controllers.

Controllers:

- AuthController
- TasksController

HTTP status codes:

- 200 OK
- 201 Created
- 204 No Content
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden when applicable
- 404 Not Found
- 409 Conflict for duplicated email or business rule conflicts

Implement global exception handling middleware.

Return a consistent error response shape, for example:

{
"error": "string",
"details": "string"
}

Add Swagger/OpenAPI with JWT bearer support.

Configure CORS for the local frontend development URL.

Controllers must be thin:

- Accept request
- Read authenticated user id from claims
- Call use case asynchronously using ExecuteAsync
- Pass CancellationToken to use case
- Return HTTP response

Controller actions that call use cases must use async Task<IActionResult> or async Task<ActionResult<T>>.

Controllers must not contain business logic.

==================================================
FRONTEND REQUIREMENTS
==================================================

Use:

- React
- TypeScript
- Vite
- Tailwind CSS

Pages:

- Login page
- Register page
- Task list page
- Create task page or modal/form
- Edit task page or modal/form

Frontend behavior:

- Store JWT in localStorage for this technical exercise
- Attach JWT to protected API requests
- Redirect unauthenticated users to login
- Show loading states
- Show validation errors
- Show API errors
- Full CRUD for tasks
- Prevent deleting completed tasks in the UI
- Still enforce completed-task deletion rule in the backend
- Use responsive layout with Tailwind CSS
- Keep components clean and organized
- Avoid browser console warnings
- Use simple state management with React Context or local state
- Do not use Redux unless absolutely necessary

Suggested frontend structure:

frontend/
├── src/
│ ├── api/
│ │ └── client.ts
│ ├── auth/
│ │ ├── AuthContext.tsx
│ │ └── ProtectedRoute.tsx
│ ├── pages/
│ │ ├── LoginPage.tsx
│ │ ├── RegisterPage.tsx
│ │ ├── TaskListPage.tsx
│ │ ├── CreateTaskPage.tsx
│ │ └── EditTaskPage.tsx
│ ├── components/
│ │ ├── TaskForm.tsx
│ │ ├── TaskCard.tsx
│ │ └── Layout.tsx
│ ├── types/
│ │ └── task.ts
│ ├── App.tsx
│ └── main.tsx

==================================================
TESTING REQUIREMENTS
==================================================

Use xUnit.

Write only meaningful tests.

Do not write generic or trivial tests, such as:

- "constructor sets property"
- "getter returns value"
- tests that only mirror implementation without validating behavior

Every test must validate a real business rule, security rule, or use case behavior.

Use the test naming pattern:

MethodName_Scenario_ExpectedBehavior

Domain tests:

- Task title cannot be empty or whitespace
- DueDate cannot be set to a past date
- Invalid status transition throws a domain exception
- Completed task cannot be deleted

Application/use case tests with mocked dependencies:

- CreateTaskUseCase creates task scoped to authenticated user
- GetTasksUseCase returns only tasks belonging to the authenticated user
- GetTaskByIdUseCase does not return another user's task
- DeleteTaskUseCase rejects deletion of a completed task
- RegisterUserUseCase rejects duplicate email
- LoginUseCase rejects invalid credentials
- UpdateTaskUseCase rejects invalid status transition

API tests, integration-style or controller tests:

- Unauthenticated request to GET /api/tasks returns 401
- Authenticated user can create a task and receives 201
- Authenticated user cannot access another user's task and receives either 403 or 404 according to the documented decision

Data access layer tests:

Add minimal repository integration tests for the data access layer using PostgreSQL.

Keep them small and focused.

Required repository tests:

- UserRepository can create and find a user by email
- TaskRepository can create and retrieve a task
- TaskRepository queries are scoped by user id and do not return another user's tasks

Mock all dependencies in use case tests.

Async test requirements:

- Tests for async methods must use async Task.
- Do not block async code in tests.
- Do not use .Result, .Wait(), or GetAwaiter().GetResult() in tests.
- Await all async calls.

==================================================
TDD APPROACH
==================================================

Follow a TDD-inspired workflow for core business rules and use cases:

1. Write tests for domain rules and use case behavior.
2. Implement the minimum code required to pass.
3. Refactor while keeping tests green.

Document this workflow in README.md and docs/thought-process.md.

Be honest and concise.

Do not claim full strict TDD if only the core rules were developed test-first.
Use the phrase "TDD-inspired" where appropriate.

==================================================
GIT COMMIT STRATEGY
==================================================

Structure the implementation as a sequence of small, meaningful commits that simulate a real human development workflow.

Do not generate everything in one commit.

Use conventional commit format.

Allowed commit types:

- feat
- test
- refactor
- docs
- chore
- fix
- style

Recommended commit sequence:

1. chore: initialize solution structure and projects
2. docs: add user story and thought process documentation
3. chore: add docker-compose, env example, and PostgreSQL init script
4. feat: add domain entities User and TaskItem
5. feat: add TaskStatus enum and domain validation rules
6. test: add domain tests for TaskItem title and due date validation
7. test: add domain tests for status transition rules
8. feat: add application DTOs and use case interfaces
9. feat: implement RegisterUserUseCase
10. test: add tests for RegisterUserUseCase duplicate email rejection
11. feat: implement LoginUseCase
12. test: add tests for LoginUseCase invalid credentials
13. feat: implement CreateTaskUseCase
14. test: add tests for CreateTaskUseCase scoped to authenticated user
15. feat: implement remaining task use cases
16. test: add tests for task ownership, deletion, and status transitions
17. feat: implement DbConnectionFactory and UserRepository
18. feat: implement TaskRepository with parameterized ADO.NET async queries
19. test: add minimal repository integration tests
20. feat: implement JWT token service
21. feat: implement password hashing service
22. feat: add AuthController with register and login endpoints
23. feat: add TasksController with full CRUD endpoints
24. feat: add global exception handling middleware
25. feat: add Swagger with JWT bearer support
26. feat: configure CORS for frontend development
27. feat: scaffold React, TypeScript, Vite, and Tailwind frontend
28. feat: implement login and register pages
29. feat: implement task list, create, and edit pages
30. feat: add protected routes and JWT auth context
31. style: apply Tailwind responsive styling to frontend pages
32. refactor: review and clean up async flow and use case implementations
33. refactor: review and clean up repository implementations
34. docs: complete README with architecture, setup, API docs, and trade-offs
35. docs: add ai-usage.md with prompt, output sample, validation, and review notes

==================================================
README REQUIREMENTS
==================================================

Create a complete, well-structured README.md with the following sections:

# Task Management System

## Overview

Brief description of the project and its purpose.

## User Story

Include the informal user story.

## Architecture

Explain Clean Architecture layers:

- Domain
- Application
- Infrastructure
- API
- Frontend

Include a simple ASCII diagram showing layer dependencies.

Explain why Entity Framework, Dapper, and MediatR were intentionally avoided.

## Tech Stack

List all technologies used:

- Backend
- Frontend
- Database
- Testing
- Tooling

## Project Structure

Show the full folder structure with a brief description of each folder.

## Async Design

Explain:

- why async/await is used across the backend request flow
- how CancellationToken is passed from controllers to use cases to repositories
- why domain methods remain synchronous
- that Npgsql async APIs are used for database operations

## How to Run

### Prerequisites

- Docker
- .NET 8 SDK
- Node.js 20+

### 1. Start the database

Run:

docker-compose up -d

### 2. Run the backend

Run:

cd backend
dotnet restore
dotnet run --project TaskManagementSystem.Api

### 3. Run the frontend

Run:

cd frontend
npm install
npm run dev

### 4. Run tests

Run:

cd backend
dotnet test

## Demo Credentials

Email: demo@taskmanagement.local
Password: Demo@12345

## API Endpoints

List all endpoints with:

- HTTP method
- Path
- Auth requirement
- Description

## Design Decisions

Explain key decisions:

- Use Case pattern
- ADO.NET with Npgsql
- Async repository operations
- JWT authentication
- Password hashing
- LocalStorage token trade-off
- 403 vs 404 decision for unauthorized task access
- Tailwind CSS for frontend styling
- React Context/local state instead of Redux

## Trade-offs

Honest list of trade-offs, for example:

- No refresh tokens
- No pagination
- No advanced role-based authorization
- JWT in localStorage only for exercise simplicity
- Minimal repository integration tests
- No advanced observability/logging

## TDD Approach

Explain the TDD-inspired workflow:

- tests first for selected domain rules
- tests for key use cases
- implementation after tests
- refactoring after tests pass

## GenAI Usage

Summarize:

- how GenAI tools were used
- what prompt was used
- what was validated
- what was corrected
- how edge cases were reviewed
- how security/authentication was reviewed

## Known Limitations

List known gaps or simplifications.

## Future Improvements

List what would be added in a production system, for example:

- refresh tokens
- httpOnly cookies or BFF pattern
- pagination/filtering/sorting
- audit logs
- better observability
- rate limiting
- more integration and end-to-end tests

==================================================
DOCUMENTATION REQUIREMENTS
==================================================

Create docs/user-story.md containing:

- the user story
- acceptance criteria
- main flows

Create docs/thought-process.md containing:

- initial interpretation of the challenge
- architectural reasoning
- business rule decisions
- async design decisions
- frontend decisions
- testing strategy
- trade-offs
- what was intentionally not implemented and why

Create docs/ai-usage.md containing:

- the prompt used to generate the scaffold or implementation
- a representative sample of AI-generated output
- how the generated code was validated
- what was manually corrected
- how edge cases were handled
- how authentication and validations were reviewed
- what security concerns were checked
- why Entity Framework, Dapper, and MediatR were intentionally avoided
- how async/await usage was reviewed
- how final code was reviewed before submission

==================================================
IMPLEMENTATION DEPTH
==================================================

Do not provide only skeletons.

Fully implement:

- Authentication flow end-to-end
- User registration
- User login
- JWT generation and validation
- Password hashing and verification
- Task CRUD backend
- Task ownership validation
- Task status transition validation
- Completed task deletion protection
- Async controller actions
- Async use case execution methods
- Async repository methods
- Npgsql async database operations
- CancellationToken propagation where reasonable
- React login page
- React register page
- React task list page
- React create task flow
- React edit task flow
- React delete task flow
- Protected frontend routes
- PostgreSQL schema
- Seed data
- Docker Compose
- .env.example
- Global exception handling middleware
- Swagger with JWT bearer support
- CORS for frontend development
- Meaningful business rule tests
- Minimal repository integration tests
- Complete README
- Documentation files in docs/

Keep the implementation simple, robust, and reviewable.

Prefer clarity over cleverness.

Every file should be ready for code review.

==================================================
FINAL SELF-REVIEW
==================================================

After generating the solution, perform a final self-review and list:

- Any architectural weaknesses
- Any missing or incomplete requirement
- Any security trade-off
- Any testing gap
- Any frontend limitation
- Any setup risk
- Any async/await inconsistency
- Any synchronous database call accidentally introduced
- Any use of .Result, .Wait(), or GetAwaiter().GetResult()
- Suggested future improvements

Be honest and practical.
