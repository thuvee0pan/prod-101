'use client';

import type { Warning } from '@/types/api';

interface Props {
  warnings: Warning[];
  onAcknowledge: (id: string) => void;
}

export function WarningBanner({ warnings, onAcknowledge }: Props) {
  if (warnings.length === 0) return null;

  return (
    <div className="space-y-2 mb-6">
      {warnings.map((w) => (
        <div
          key={w.id}
          className="bg-danger/10 border border-danger/30 rounded-lg p-4 flex items-start justify-between"
        >
          <div>
            <p className="text-danger text-sm font-medium uppercase tracking-wide">
              {w.warningType === 'no_daily_log' ? 'INACTIVITY DETECTED' : 'STALE PROJECT'}
            </p>
            <p className="text-sm text-white/80 mt-1">{w.message}</p>
          </div>
          <button
            onClick={() => onAcknowledge(w.id)}
            className="text-xs text-danger border border-danger/30 px-3 py-1 rounded hover:bg-danger/20 transition-colors shrink-0 ml-4"
          >
            Acknowledge
          </button>
        </div>
      ))}
    </div>
  );
}
