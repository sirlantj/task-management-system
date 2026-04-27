-- Task Management System Database Schema

-- Users table
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(500) NOT NULL,
    password_salt VARCHAR(500) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Tasks table
CREATE TABLE IF NOT EXISTS tasks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(500) NOT NULL,
    description TEXT,
    status VARCHAR(50) NOT NULL CHECK (status IN ('Pending', 'InProgress', 'Done')),
    due_date TIMESTAMPTZ,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    CONSTRAINT check_title_not_empty CHECK (LENGTH(TRIM(title)) > 0)
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_tasks_user_id ON tasks(user_id);
CREATE INDEX IF NOT EXISTS idx_tasks_status ON tasks(status);
CREATE INDEX IF NOT EXISTS idx_tasks_due_date ON tasks(due_date);
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);

-- Seed data: Demo user
-- Email: demo@example.com
-- Password: Demo@123
-- BCrypt hash with work factor 11
INSERT INTO users (id, name, email, password_hash, password_salt, created_at)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'Demo User',
    'demo@example.com',
    '$2a$11$qp.4RQGXNzNNWCwCHKj5SeLp9JaVMlS1VrJNzQUzLXk6LN/rVWZnK',
    '',
    NOW()
) ON CONFLICT (email) DO NOTHING;

-- Seed data: Sample tasks for demo user
INSERT INTO tasks (title, description, status, due_date, user_id, created_at)
VALUES
    (
        'Complete project documentation',
        'Write comprehensive documentation for the task management system',
        'InProgress',
        NOW() + INTERVAL '7 days',
        '00000000-0000-0000-0000-000000000001',
        NOW()
    ),
    (
        'Review pull requests',
        'Review and merge pending pull requests from team members',
        'Pending',
        NOW() + INTERVAL '2 days',
        '00000000-0000-0000-0000-000000000001',
        NOW()
    ),
    (
        'Setup CI/CD pipeline',
        NULL,
        'Done',
        NOW() - INTERVAL '1 day',
        '00000000-0000-0000-0000-000000000001',
        NOW() - INTERVAL '2 days'
    )
ON CONFLICT DO NOTHING;
