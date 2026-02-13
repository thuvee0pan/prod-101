'use client';

import { useEffect, useState } from 'react';
import type { TodoItem } from '@/types/api';
import { getTodosByDate, createTodo, updateTodo, deleteTodo } from '@/lib/api';

const CATEGORIES = ['Work', 'Personal', 'Gym', 'Learning', 'Health', 'Finance', 'Social', 'Other'];

const CATEGORY_COLORS: Record<string, string> = {
  Work: 'bg-blue-500/20 text-blue-400 border-blue-500/30',
  Personal: 'bg-purple-500/20 text-purple-400 border-purple-500/30',
  Gym: 'bg-green-500/20 text-green-400 border-green-500/30',
  Learning: 'bg-amber-500/20 text-amber-400 border-amber-500/30',
  Health: 'bg-pink-500/20 text-pink-400 border-pink-500/30',
  Finance: 'bg-emerald-500/20 text-emerald-400 border-emerald-500/30',
  Social: 'bg-cyan-500/20 text-cyan-400 border-cyan-500/30',
  Other: 'bg-gray-500/20 text-gray-400 border-gray-500/30',
};

function todayStr() {
  return new Date().toISOString().split('T')[0];
}

export default function TodosPage() {
  const [todos, setTodos] = useState<TodoItem[]>([]);
  const [selectedDate, setSelectedDate] = useState(todayStr());
  const [filterCategory, setFilterCategory] = useState<string>('All');
  const [error, setError] = useState<string | null>(null);

  // New todo form
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState('Personal');

  // Editing state
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editTitle, setEditTitle] = useState('');
  const [editDescription, setEditDescription] = useState('');
  const [editCategory, setEditCategory] = useState('');

  const load = async () => {
    try {
      const data = await getTodosByDate(selectedDate);
      setTodos(data);
    } catch (e: any) {
      setError(e.message);
    }
  };

  useEffect(() => { load(); }, [selectedDate]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) return;
    setError(null);
    try {
      await createTodo({ title: title.trim(), description: description.trim() || undefined, category, dueDate: selectedDate });
      setTitle('');
      setDescription('');
      load();
    } catch (e: any) {
      setError(e.message);
    }
  };

  const handleToggleStatus = async (todo: TodoItem) => {
    const next = todo.status === 'Done' ? 'Pending' : 'Done';
    await updateTodo(todo.id, { status: next });
    load();
  };

  const handleStartEdit = (todo: TodoItem) => {
    setEditingId(todo.id);
    setEditTitle(todo.title);
    setEditDescription(todo.description || '');
    setEditCategory(todo.category);
  };

  const handleSaveEdit = async (id: string) => {
    try {
      await updateTodo(id, {
        title: editTitle.trim(),
        description: editDescription.trim() || undefined,
        category: editCategory,
      });
      setEditingId(null);
      load();
    } catch (e: any) {
      setError(e.message);
    }
  };

  const handleDelete = async (id: string) => {
    await deleteTodo(id);
    load();
  };

  const navigateDay = (offset: number) => {
    const d = new Date(selectedDate + 'T00:00:00');
    d.setDate(d.getDate() + offset);
    setSelectedDate(d.toISOString().split('T')[0]);
  };

  const isToday = selectedDate === todayStr();

  const filtered = filterCategory === 'All'
    ? todos
    : todos.filter(t => t.category === filterCategory);

  const pending = filtered.filter(t => t.status !== 'Done');
  const done = filtered.filter(t => t.status === 'Done');

  const totalCount = todos.length;
  const doneCount = todos.filter(t => t.status === 'Done').length;

  return (
    <div className="max-w-2xl">
      <h1 className="text-2xl font-bold mb-1">Todos</h1>
      <p className="text-muted text-sm mb-6">Plan your day. Execute. Check off.</p>

      {error && <p className="text-danger text-sm mb-4">{error}</p>}

      {/* Date navigation */}
      <div className="flex items-center gap-3 mb-6">
        <button onClick={() => navigateDay(-1)} className="text-muted hover:text-white px-2 py-1 rounded hover:bg-border/50 transition-colors">&larr;</button>
        <div className="flex items-center gap-2">
          <input
            type="date"
            value={selectedDate}
            onChange={(e) => setSelectedDate(e.target.value)}
            className="input w-auto bg-transparent border-none text-white text-center cursor-pointer"
          />
          {isToday && <span className="text-xs bg-accent/20 text-accent px-2 py-0.5 rounded">TODAY</span>}
        </div>
        <button onClick={() => navigateDay(1)} className="text-muted hover:text-white px-2 py-1 rounded hover:bg-border/50 transition-colors">&rarr;</button>
        {!isToday && (
          <button onClick={() => setSelectedDate(todayStr())} className="text-xs text-accent hover:underline ml-2">
            Go to today
          </button>
        )}
      </div>

      {/* Progress bar */}
      {totalCount > 0 && (
        <div className="mb-6">
          <div className="flex justify-between text-xs text-muted mb-1">
            <span>{doneCount} of {totalCount} done</span>
            <span>{Math.round((doneCount / totalCount) * 100)}%</span>
          </div>
          <div className="h-2 bg-border rounded-full overflow-hidden">
            <div
              className="h-full bg-accent rounded-full transition-all duration-300"
              style={{ width: `${(doneCount / totalCount) * 100}%` }}
            />
          </div>
        </div>
      )}

      {/* Add todo form */}
      <form onSubmit={handleCreate} className="card mb-6">
        <div className="flex gap-3 items-start">
          <div className="flex-1">
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="input mb-2"
              placeholder="What needs to get done?"
              required
            />
            <input
              type="text"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="input text-sm mb-2"
              placeholder="Description (optional)"
            />
            <div className="flex gap-2 items-center">
              <select
                value={category}
                onChange={(e) => setCategory(e.target.value)}
                className="input w-auto text-sm"
              >
                {CATEGORIES.map(c => (
                  <option key={c} value={c}>{c}</option>
                ))}
              </select>
              <button type="submit" className="btn-primary text-sm">
                Add
              </button>
            </div>
          </div>
        </div>
      </form>

      {/* Category filter */}
      <div className="flex gap-2 mb-4 flex-wrap">
        {['All', ...CATEGORIES].map(c => (
          <button
            key={c}
            onClick={() => setFilterCategory(c)}
            className={`text-xs px-2.5 py-1 rounded-full border transition-colors ${
              filterCategory === c
                ? 'bg-accent/20 text-accent border-accent/30'
                : 'text-muted border-border hover:border-muted'
            }`}
          >
            {c}
          </button>
        ))}
      </div>

      {/* Pending todos */}
      {pending.length > 0 && (
        <div className="space-y-2 mb-6">
          {pending.map(todo => (
            <div key={todo.id} className="card flex items-start gap-3 group">
              {editingId === todo.id ? (
                <div className="flex-1">
                  <input
                    type="text"
                    value={editTitle}
                    onChange={(e) => setEditTitle(e.target.value)}
                    className="input mb-2 text-sm"
                    autoFocus
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') handleSaveEdit(todo.id);
                      if (e.key === 'Escape') setEditingId(null);
                    }}
                  />
                  <input
                    type="text"
                    value={editDescription}
                    onChange={(e) => setEditDescription(e.target.value)}
                    className="input mb-2 text-xs"
                    placeholder="Description"
                  />
                  <div className="flex gap-2 items-center">
                    <select
                      value={editCategory}
                      onChange={(e) => setEditCategory(e.target.value)}
                      className="input w-auto text-xs"
                    >
                      {CATEGORIES.map(c => (
                        <option key={c} value={c}>{c}</option>
                      ))}
                    </select>
                    <button onClick={() => handleSaveEdit(todo.id)} className="btn-primary text-xs px-3 py-1">Save</button>
                    <button onClick={() => setEditingId(null)} className="text-xs text-muted hover:text-white">Cancel</button>
                  </div>
                </div>
              ) : (
                <>
                  <button
                    onClick={() => handleToggleStatus(todo)}
                    className="mt-1 w-5 h-5 rounded border-2 border-muted hover:border-accent flex-shrink-0 transition-colors"
                  />
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <span className="text-sm text-white">{todo.title}</span>
                      <span className={`text-[10px] px-1.5 py-0.5 rounded border ${CATEGORY_COLORS[todo.category] || CATEGORY_COLORS.Other}`}>
                        {todo.category}
                      </span>
                    </div>
                    {todo.description && (
                      <p className="text-xs text-muted mt-0.5">{todo.description}</p>
                    )}
                  </div>
                  <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                    <button
                      onClick={() => handleStartEdit(todo)}
                      className="text-xs text-muted hover:text-white px-1.5 py-0.5 rounded hover:bg-border/50"
                    >
                      Edit
                    </button>
                    <button
                      onClick={() => handleDelete(todo.id)}
                      className="text-xs text-muted hover:text-danger px-1.5 py-0.5 rounded hover:bg-danger/10"
                    >
                      Del
                    </button>
                  </div>
                </>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Done todos */}
      {done.length > 0 && (
        <div>
          <h3 className="text-xs text-muted uppercase tracking-widest mb-3">COMPLETED ({done.length})</h3>
          <div className="space-y-2">
            {done.map(todo => (
              <div key={todo.id} className="card flex items-start gap-3 opacity-50 group">
                <button
                  onClick={() => handleToggleStatus(todo)}
                  className="mt-1 w-5 h-5 rounded border-2 border-accent bg-accent/20 flex-shrink-0 flex items-center justify-center transition-colors"
                >
                  <svg className="w-3 h-3 text-accent" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={3}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                  </svg>
                </button>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <span className="text-sm text-muted line-through">{todo.title}</span>
                    <span className={`text-[10px] px-1.5 py-0.5 rounded border ${CATEGORY_COLORS[todo.category] || CATEGORY_COLORS.Other}`}>
                      {todo.category}
                    </span>
                  </div>
                  {todo.description && (
                    <p className="text-xs text-muted mt-0.5 line-through">{todo.description}</p>
                  )}
                </div>
                <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                  <button
                    onClick={() => handleDelete(todo.id)}
                    className="text-xs text-muted hover:text-danger px-1.5 py-0.5 rounded hover:bg-danger/10"
                  >
                    Del
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Empty state */}
      {filtered.length === 0 && (
        <div className="text-center py-12">
          <p className="text-muted text-sm">
            {filterCategory !== 'All'
              ? `No ${filterCategory.toLowerCase()} todos for this day.`
              : 'No todos yet. Add one above.'}
          </p>
        </div>
      )}
    </div>
  );
}
