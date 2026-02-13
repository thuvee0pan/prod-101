'use client';

import { useEffect, useState } from 'react';
import type { Dashboard } from '@/types/api';
import { getDashboard, acknowledgeWarning } from '@/lib/api';
import { StreakBar } from '@/components/StreakBar';
import { WarningBanner } from '@/components/WarningBanner';
import { ScoreRing } from '@/components/ScoreRing';

export default function DashboardPage() {
  const [data, setData] = useState<Dashboard | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = () => {
    getDashboard().then(setData).catch((e) => setError(e.message));
  };

  useEffect(() => { load(); }, []);

  const handleAcknowledge = async (id: string) => {
    await acknowledgeWarning(id);
    load();
  };

  if (error) {
    return (
      <div className="max-w-4xl">
        <h1 className="text-2xl font-bold mb-2">Dashboard</h1>
        <p className="text-muted mb-6">Connect the backend to see your data.</p>
        <div className="card">
          <p className="text-danger text-sm">{error}</p>
          <p className="text-muted text-sm mt-2">
            Make sure the .NET API is running on localhost:5000
          </p>
        </div>
      </div>
    );
  }

  if (!data) return <div className="text-muted">Loading...</div>;

  return (
    <div className="max-w-4xl">
      <WarningBanner warnings={data.activeWarnings} onAcknowledge={handleAcknowledge} />

      {/* Goal Progress */}
      <section className="mb-8">
        <h2 className="text-xs text-muted uppercase tracking-widest mb-3">90-DAY GOAL</h2>
        {data.activeGoal ? (
          <div className="card">
            <h3 className="text-lg font-bold text-white">{data.activeGoal.title}</h3>
            <p className="text-sm text-muted mt-1">{data.activeGoal.description}</p>
            <div className="mt-4 flex items-center gap-6">
              <div>
                <p className="text-2xl font-bold text-accent">{data.activeGoal.daysElapsed}</p>
                <p className="text-xs text-muted">days in</p>
              </div>
              <div>
                <p className="text-2xl font-bold text-warning">{data.activeGoal.daysRemaining}</p>
                <p className="text-xs text-muted">days left</p>
              </div>
              <div className="flex-1">
                <div className="h-3 bg-border rounded-full overflow-hidden">
                  <div
                    className="h-full bg-accent rounded-full transition-all"
                    style={{ width: `${Math.min(100, (data.activeGoal.daysElapsed / 90) * 100)}%` }}
                  />
                </div>
              </div>
            </div>
          </div>
        ) : (
          <div className="card border-dashed">
            <p className="text-muted">No active goal. Set one to start your 90-day sprint.</p>
          </div>
        )}
      </section>

      {/* Score + Projects */}
      <div className="grid grid-cols-2 gap-6 mb-8">
        <section>
          <h2 className="text-xs text-muted uppercase tracking-widest mb-3">WEEKLY SCORE</h2>
          <div className="card flex justify-center">
            <ScoreRing percentage={data.score.overallPercentage} label="This Week" />
          </div>
          <div className="grid grid-cols-2 gap-2 mt-3">
            <div className="card text-center py-3">
              <p className="text-lg font-bold">{Math.round(data.score.weeklyDeepWorkMinutes / 60)}h</p>
              <p className="text-xs text-muted">Deep Work</p>
            </div>
            <div className="card text-center py-3">
              <p className="text-lg font-bold">{data.score.weeklyGymDays}/7</p>
              <p className="text-xs text-muted">Gym Days</p>
            </div>
            <div className="card text-center py-3">
              <p className="text-lg font-bold">{Math.round(data.score.weeklyLearningMinutes / 60)}h</p>
              <p className="text-xs text-muted">Learning</p>
            </div>
            <div className="card text-center py-3">
              <p className="text-lg font-bold">{data.score.weeklySoberDays}/7</p>
              <p className="text-xs text-muted">Sober Days</p>
            </div>
          </div>
        </section>

        <section>
          <h2 className="text-xs text-muted uppercase tracking-widest mb-3">
            ACTIVE PROJECTS ({data.activeProjects.length}/2)
          </h2>
          {data.activeProjects.length > 0 ? (
            <div className="space-y-3">
              {data.activeProjects.map((p) => (
                <div key={p.id} className="card">
                  <h3 className="font-medium text-white">{p.title}</h3>
                  <p className="text-sm text-muted mt-1 line-clamp-2">{p.description}</p>
                </div>
              ))}
            </div>
          ) : (
            <div className="card border-dashed">
              <p className="text-muted">No active projects.</p>
            </div>
          )}
        </section>
      </div>

      {/* Streaks */}
      <section className="mb-8">
        <h2 className="text-xs text-muted uppercase tracking-widest mb-3">STREAKS</h2>
        <div className="card">
          {data.streaks.map((s) => (
            <StreakBar key={s.streakType} streak={s} />
          ))}
        </div>
      </section>

      {/* Today's Log */}
      <section>
        <h2 className="text-xs text-muted uppercase tracking-widest mb-3">TODAY</h2>
        {data.todayLog ? (
          <div className="card">
            <div className="grid grid-cols-4 gap-4 text-center">
              <div>
                <p className="text-xl font-bold">{data.todayLog.deepWorkMinutes}m</p>
                <p className="text-xs text-muted">Deep Work</p>
              </div>
              <div>
                <p className={`text-xl font-bold ${data.todayLog.gymCompleted ? 'text-accent' : 'text-danger'}`}>
                  {data.todayLog.gymCompleted ? 'YES' : 'NO'}
                </p>
                <p className="text-xs text-muted">Gym</p>
              </div>
              <div>
                <p className="text-xl font-bold">{data.todayLog.learningMinutes}m</p>
                <p className="text-xs text-muted">Learning</p>
              </div>
              <div>
                <p className={`text-xl font-bold ${data.todayLog.alcoholFree ? 'text-accent' : 'text-danger'}`}>
                  {data.todayLog.alcoholFree ? 'CLEAN' : 'DRANK'}
                </p>
                <p className="text-xs text-muted">Alcohol</p>
              </div>
            </div>
          </div>
        ) : (
          <div className="card border-dashed">
            <p className="text-muted">No log today. Go log your execution.</p>
          </div>
        )}
      </section>
    </div>
  );
}
