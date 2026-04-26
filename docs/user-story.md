# User Story

## Story

> "As a registered user, I want to securely manage my own tasks so that I can organize my work, track progress, and keep my personal task list private."

## Acceptance Criteria

- A user can register with a name, email, and password.
- A user can log in and receive a JWT for subsequent requests.
- A user can create a task with a title, optional description, optional due date, and initial status Pending.
- A user can view all of their own tasks.
- A user can view a single task by ID.
- A user can update a task's title, description, due date, or status.
- A user can delete a task that is not in Done status.
- A user cannot access, update, or delete tasks belonging to another user.
- Task title cannot be empty or whitespace.
- Due date cannot be set to a date in the past.
- Status transitions must follow: Pending → InProgress, Pending → Done, InProgress → Done. Done is terminal.
- Completed (Done) tasks cannot be deleted.

## Main Flows

### Registration
1. User submits name, email, and password.
2. System validates uniqueness of email.
3. System hashes password with salt and persists the user.
4. System returns a JWT.

### Login
1. User submits email and password.
2. System verifies credentials.
3. System returns a JWT on success, or 401 on failure.

### Task CRUD
1. Authenticated user creates a task — task is scoped to their user ID.
2. User retrieves their task list — only their tasks are returned.
3. User updates a task — ownership and business rules are enforced.
4. User deletes a task — rejected if status is Done.
