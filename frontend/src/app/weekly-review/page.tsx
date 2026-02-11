'use client';

import { useEffect, useState } from 'react';
import type { WeeklyReview } from '@/types/api';
import { getAllReviews, generateWeeklyReview } from '@/lib/api';

export default function WeeklyReviewPage() {
  const [reviews, setReviews] = useState<WeeklyReview[]>([]);
  const [generating, setGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    setReviews(await getAllReviews().catch(() => []));
  };

  useEffect(() => { load(); }, []);

  const handleGenerate = async () => {
    setGenerating(true);
    setError(null);
    try {
      await generateWeeklyReview();
      load();
    } catch (e: any) {
      setError(e.message);
    } finally {
      setGenerating(false);
    }
  };

  return (
    <div className="max-w-2xl">
      <h1 className="text-2xl font-bold mb-1">Weekly Review</h1>
      <p className="text-muted text-sm mb-6">AI-generated accountability. No sugar-coating.</p>

      <div className="mb-8">
        <button
          onClick={handleGenerate}
          disabled={generating}
          className="btn-primary"
        >
          {generating ? 'Analyzing...' : 'Generate This Week\'s Review'}
        </button>
        {error && <p className="text-danger text-sm mt-2">{error}</p>}
      </div>

      {reviews.length === 0 ? (
        <div className="card border-dashed">
          <p className="text-muted">No reviews yet. Generate your first one.</p>
        </div>
      ) : (
        <div className="space-y-6">
          {reviews.map((review) => (
            <div key={review.id} className="card">
              <div className="flex justify-between items-center mb-4">
                <h2 className="text-sm font-medium text-white">
                  Week of {review.weekStart} to {review.weekEnd}
                </h2>
                <span className="text-xs text-muted">
                  {new Date(review.generatedAt).toLocaleDateString()}
                </span>
              </div>

              <div className="space-y-4">
                <div>
                  <h3 className="text-xs text-accent uppercase tracking-widest mb-2">
                    WHAT WORKED
                  </h3>
                  <p className="text-sm text-white/80">{review.whatWorked || 'No data.'}</p>
                </div>

                <div>
                  <h3 className="text-xs text-danger uppercase tracking-widest mb-2">
                    WHERE YOU AVOIDED HARD WORK
                  </h3>
                  <p className="text-sm text-white/80">{review.whereAvoided || 'No data.'}</p>
                </div>

                <div>
                  <h3 className="text-xs text-warning uppercase tracking-widest mb-2">
                    WHAT TO CUT
                  </h3>
                  <p className="text-sm text-white/80">{review.whatToCut || 'No data.'}</p>
                </div>
              </div>

              <details className="mt-4">
                <summary className="text-xs text-muted cursor-pointer hover:text-white transition-colors">
                  Full AI Response
                </summary>
                <pre className="text-xs text-muted mt-2 whitespace-pre-wrap bg-bg p-3 rounded-md">
                  {review.aiSummary}
                </pre>
              </details>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
