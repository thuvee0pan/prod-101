'use client';

import type { Streak } from '@/types/api';

const streakLabels: Record<string, string> = {
  DeepWork: 'Deep Work',
  Gym: 'Gym',
  Learning: 'Learning',
  Sober: 'Alcohol-Free',
};

const streakColors: Record<string, string> = {
  DeepWork: 'bg-blue-500',
  Gym: 'bg-green-500',
  Learning: 'bg-purple-500',
  Sober: 'bg-amber-500',
};

export function StreakBar({ streak }: { streak: Streak }) {
  const label = streakLabels[streak.streakType] || streak.streakType;
  const color = streakColors[streak.streakType] || 'bg-accent';
  const percentage = streak.longestCount > 0
    ? Math.min(100, (streak.currentCount / streak.longestCount) * 100)
    : streak.currentCount > 0 ? 100 : 0;

  return (
    <div className="mb-4">
      <div className="flex justify-between items-center mb-1">
        <span className="text-sm text-white">{label}</span>
        <span className="text-sm">
          <span className={streak.currentCount > 0 ? 'streak-fire' : 'text-muted'}>
            {streak.currentCount}d
          </span>
          <span className="text-muted ml-1">/ best {streak.longestCount}d</span>
        </span>
      </div>
      <div className="h-2 bg-border rounded-full overflow-hidden">
        <div
          className={`h-full ${color} rounded-full transition-all duration-500`}
          style={{ width: `${percentage}%` }}
        />
      </div>
    </div>
  );
}
