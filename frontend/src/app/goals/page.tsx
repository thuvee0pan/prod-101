'use client';

import { useEffect, useState } from 'react';
import type { Goal } from '@/types/api';
import { getActiveGoal, getAllGoals, createGoal, completeGoal, abandonGoal } from '@/lib/api';

export default function GoalsPage() {
  const [activeGoal, setActiveGoal] = useState<Goal | null>(null);
  const [allGoals, setAllGoals] = useState<Goal[]>([]);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [abandonReason, setAbandonReason] = useState('');
  const [showAbandon, setShowAbandon] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    try {
      const active = await getActiveGoal().catch(() => null);
      setActiveGoal(active);
      const all = await getAllGoals().catch(() => []);
      setAllGoals(all);
    } catch (e: any) {
      setError(e.message);
    }
  };

  useEffect(() => { load(); }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await createGoal(title, description);
      setTitle('');
      setDescription('');
      load();
    } catch (e: any) {
      setError(e.message);
    }
  };

  const handleComplete = async () => {
    if (!activeGoal) return;
    await completeGoal(activeGoal.id);
    load();
  };

  const handleAbandon = async () => {
    if (!activeGoal || !abandonReason) return;
    await abandonGoal(activeGoal.id, abandonReason);
    setShowAbandon(false);
    setAbandonReason('');
    load();
  };

  return (
    <div className="max-w-2xl">
      <h1 className="text-2xl font-bold mb-1">90-Day Goal</h1>
      <p className="text-muted text-sm mb-6">One goal. That&apos;s it. Pick the thing that matters most.</p>

      {error && <p className="text-danger text-sm mb-4">{error}</p>}

      {activeGoal ? (
        <div className="card mb-8">
          <div className="flex items-start justify-between">
            <div>
              <h2 className="text-lg font-bold text-white">{activeGoal.title}</h2>
              <p className="text-sm text-muted mt-1">{activeGoal.description}</p>
            </div>
            <span className="text-xs bg-accent/20 text-accent px-2 py-1 rounded">ACTIVE</span>
          </div>

          <div className="mt-4">
            <div className="h-3 bg-border rounded-full overflow-hidden mb-2">
              <div
                className="h-full bg-accent rounded-full"
                style={{ width: `${Math.min(100, (activeGoal.daysElapsed / 90) * 100)}%` }}
              />
            </div>
            <div className="flex justify-between text-xs text-muted">
              <span>Day {activeGoal.daysElapsed} of 90</span>
              <span>{activeGoal.daysRemaining} days remaining</span>
            </div>
          </div>

          <div className="flex gap-3 mt-6">
            <button onClick={handleComplete} className="btn-primary">
              Mark Complete
            </button>
            <button onClick={() => setShowAbandon(!showAbandon)} className="btn-danger">
              Abandon
            </button>
          </div>

          {showAbandon && (
            <div className="mt-4 p-4 bg-danger/5 border border-danger/20 rounded-lg">
              <p className="text-sm text-danger mb-2">Why are you quitting? Be honest.</p>
              <textarea
                value={abandonReason}
                onChange={(e) => setAbandonReason(e.target.value)}
                className="input mb-3"
                rows={2}
                placeholder="Explain why this goal no longer serves you..."
              />
              <button onClick={handleAbandon} className="btn-danger" disabled={!abandonReason}>
                Confirm Abandon
              </button>
            </div>
          )}
        </div>
      ) : (
        <form onSubmit={handleCreate} className="card mb-8">
          <h2 className="text-sm font-medium text-white mb-4">Set Your 90-Day Goal</h2>
          <div className="mb-4">
            <label className="label">Goal Title</label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="input"
              placeholder="e.g., Launch my SaaS product"
              required
            />
          </div>
          <div className="mb-4">
            <label className="label">Description</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="input"
              rows={3}
              placeholder="What does success look like in 90 days?"
              required
            />
          </div>
          <button type="submit" className="btn-primary">
            Lock In Goal
          </button>
        </form>
      )}

      {/* Past Goals */}
      {allGoals.filter(g => g.status !== 'Active').length > 0 && (
        <section>
          <h2 className="text-xs text-muted uppercase tracking-widest mb-3">PAST GOALS</h2>
          <div className="space-y-3">
            {allGoals.filter(g => g.status !== 'Active').map((g) => (
              <div key={g.id} className="card opacity-60">
                <div className="flex justify-between items-center">
                  <span className="text-sm text-white">{g.title}</span>
                  <span className={`text-xs px-2 py-1 rounded ${
                    g.status === 'Completed' ? 'bg-accent/20 text-accent' : 'bg-danger/20 text-danger'
                  }`}>
                    {g.status}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
