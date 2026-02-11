'use client';

interface Props {
  percentage: number;
  label: string;
}

export function ScoreRing({ percentage, label }: Props) {
  const radius = 45;
  const circumference = 2 * Math.PI * radius;
  const offset = circumference - (percentage / 100) * circumference;

  const color = percentage >= 75 ? '#22c55e' : percentage >= 50 ? '#f59e0b' : '#ef4444';

  return (
    <div className="flex flex-col items-center">
      <svg width="120" height="120" className="-rotate-90">
        <circle cx="60" cy="60" r={radius} fill="none" stroke="#262626" strokeWidth="8" />
        <circle
          cx="60"
          cy="60"
          r={radius}
          fill="none"
          stroke={color}
          strokeWidth="8"
          strokeDasharray={circumference}
          strokeDashoffset={offset}
          strokeLinecap="round"
          className="transition-all duration-1000"
        />
      </svg>
      <div className="absolute mt-10 text-center">
        <p className="text-2xl font-bold" style={{ color }}>
          {percentage}%
        </p>
      </div>
      <p className="text-xs text-muted mt-2">{label}</p>
    </div>
  );
}
