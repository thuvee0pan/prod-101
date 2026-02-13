-- Personal Execution OS - Add Todos
-- Tracks daily tasks with categories

CREATE TABLE todo_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(500) NOT NULL,
    description TEXT,
    category VARCHAR(20) NOT NULL DEFAULT 'Personal' CHECK (category IN ('Work', 'Personal', 'Gym', 'Learning', 'Health', 'Finance', 'Social', 'Other')),
    status VARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending', 'InProgress', 'Done')),
    due_date DATE NOT NULL DEFAULT CURRENT_DATE,
    sort_order INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_todos_user_date ON todo_items(user_id, due_date DESC);
CREATE INDEX idx_todos_user_category ON todo_items(user_id, category);
CREATE INDEX idx_todos_user_status ON todo_items(user_id, status);
