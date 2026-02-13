'use client';

import { useEffect, useState } from 'react';
import type { DailyLog, Streak } from '@/types/api';
import { logToday, getTodayLog, getLogs, getStreaks } from '@/lib/api';
import { StreakBar } from '@/components/StreakBar';

export default function DailyLogPage() {
  const [todayLog, setTodayLog] = useState<DailyLog | null>(null);
  const [recentLogs, setRecentLogs] = useState<DailyLog[]>([]);
  const [streaks, setStreaks] = useState<Streak[]>([]);

  const [deepWork, setDeepWork] = useState(0);
  const [gym, setGym] = useState(false);
  const [learning, setLearning] = useState(0);
  const [sober, setSober] = useState(true);
  const [notes, setNotes] = useState('');
  const [saved, setSaved] = useState(false);

  const load = async () => {
    const today = await getTodayLog().catch(() => null);
    setTodayLog(today);
    if (today) {
      setDeepWork(today.deepWorkMinutes);
      setGym(today.gymCompleted);
      setLearning(today.learningMinutes);
      setSober(today.alcoholFree);
      setNotes(today.notes || '');
    }

    setRecentLogs(await getLogs().catch(() => []));
    setStreaks(await getStreaks().catch(() => []));
  };

  useEffect(() => { load(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await logToday({
      deepWorkMinutes: deepWork,
      gymCompleted: gym,
      learningMinutes: learning,
      alcoholFree: sober,
      notes: notes || undefined,
    });
    setSaved(true);
    setTimeout(() => setSaved(false), 2000);
    load();
  };

  return (
    <div className="max-w-2xl">
      <h1 className="text-2xl font-bold mb-1">Daily Log</h1>
      <p className="text-muted text-sm mb-6">What did you actually do today? No lies.</p>

      {/* Log Form */}
      <form onSubmit={handleSubmit} className="card mb-8">
        <div className="grid grid-cols-2 gap-6 mb-6">
          <div>
            <label className="label">Deep Work (minutes)</label>
            <input
              type="number"
              min={0}
              max={720}
              value={deepWork}
              onChange={(e) => setDeepWork(Number(e.target.value))}
              className="input"
            />
            <p className="text-xs text-muted mt-1">
              {deepWork >= 120 ? 'Solid.' : deepWork > 0 ? 'Not enough.' : 'Zero? Really?'}
            </p>
          </div>
          <div>
            <label className="label">Learning (minutes)</label>
            <input
              type="number"
              min={0}
              max={720}
              value={learning}
              onChange={(e) => setLearning(Number(e.target.value))}
              className="input"
            />
            <p className="text-xs text-muted mt-1">
              {learning >= 30 ? 'Good habit.' : 'Growth requires input.'}
            </p>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-6 mb-6">
          <div>
            <label className="label">Gym</label>
            <button
              type="button"
              onClick={() => setGym(!gym)}
              className={`w-full py-3 rounded-md font-medium transition-colors ${
                gym
                  ? 'bg-accent/20 text-accent border border-accent/30'
                  : 'bg-danger/10 text-danger border border-danger/30'
              }`}
            >
              {gym ? 'YES - Trained' : 'NO - Skipped'}
            </button>
          </div>
          <div>
            <label className="label">Alcohol-Free</label>
            <button
              type="button"
              onClick={() => setSober(!sober)}
              className={`w-full py-3 rounded-md font-medium transition-colors ${
                sober
                  ? 'bg-accent/20 text-accent border border-accent/30'
                  : 'bg-danger/10 text-danger border border-danger/30'
              }`}
            >
              {sober ? 'CLEAN' : 'DRANK'}
            </button>
          </div>
        </div>

        <div className="mb-6">
          <label className="label">Notes</label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            className="input"
            rows={3}
            placeholder="What did you work on? Be specific. Mention project names."
          />
        </div>

        <div className="flex items-center gap-4">
          <button type="submit" className="btn-primary">
            {todayLog ? 'Update Log' : 'Log Today'}
          </button>
          {saved && <span className="text-accent text-sm">Saved.</span>}
        </div>
      </form>

      {/* Streaks */}
      <section className="mb-8">
        <h2 className="text-xs text-muted uppercase tracking-widest mb-3">STREAKS</h2>
        <div className="card">
          {streaks.map((s) => (
            <StreakBar key={s.streakType} streak={s} />
          ))}
          {streaks.length === 0 && (
            <p className="text-muted text-sm">Log your first day to start building streaks.</p>
          )}
        </div>
      </section>

      {/* Recent History */}
      <section>
        <h2 className="text-xs text-muted uppercase tracking-widest mb-3">LAST 30 DAYS</h2>
        <div className="grid grid-cols-7 gap-1">
          {recentLogs.slice(0, 30).map((log) => {
            const score =
              (log.deepWorkMinutes >= 120 ? 1 : 0) +
              (log.gymCompleted ? 1 : 0) +
              (log.learningMinutes >= 30 ? 1 : 0) +
              (log.alcoholFree ? 1 : 0);

            const color =
              score === 4 ? 'bg-accent' :
              score === 3 ? 'bg-accent/60' :
              score === 2 ? 'bg-warning/60' :
              score === 1 ? 'bg-danger/60' :
              'bg-border';

            return (
              <div
                key={log.logDate}
                className={`${color} rounded-sm h-8 flex items-center justify-center`}
                title={`${log.logDate}: ${score}/4 metrics hit`}
              >
                <span className="text-[10px] text-white/60">{score}</span>
              </div>
            );
          })}
        </div>
        <p className="text-xs text-muted mt-2">Green = all 4 metrics hit. Red = falling behind.</p>
      </section>
    </div>
  );
}
