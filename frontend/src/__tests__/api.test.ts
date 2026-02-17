import { describe, it, expect, vi, beforeEach } from 'vitest';

// Mock fetch globally
const mockFetch = vi.fn();
global.fetch = mockFetch;

// Import after mocking
import {
  createGoal,
  getActiveGoal,
  getAllGoals,
  completeGoal,
  abandonGoal,
  createProject,
  getActiveProjects,
  updateProjectStatus,
  submitProjectChange,
  logToday,
  getTodayLog,
  getLogs,
  getStreaks,
  getWarnings,
  acknowledgeWarning,
  generateWeeklyReview,
  getAllReviews,
  getLatestReview,
  getDashboard,
} from '@/lib/api';

function mockResponse(data: unknown, status = 200) {
  return {
    ok: status >= 200 && status < 300,
    status,
    json: () => Promise.resolve(data),
  };
}

describe('API Client', () => {
  beforeEach(() => {
    mockFetch.mockReset();
  });

  describe('request headers', () => {
    it('includes Content-Type and X-User-Id headers', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse([]));

      await getAllGoals();

      expect(mockFetch).toHaveBeenCalledWith(
        '/api/goals',
        expect.objectContaining({
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
            'X-User-Id': expect.any(String),
          }),
        })
      );
    });
  });

  describe('error handling', () => {
    it('throws on non-ok response with error message', async () => {
      mockFetch.mockResolvedValueOnce(
        mockResponse({ error: 'Goal not found' }, 404)
      );

      await expect(getActiveGoal()).rejects.toThrow('Goal not found');
    });

    it('throws generic error when response has no error field', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: () => Promise.reject(new Error('invalid json')),
      });

      await expect(getActiveGoal()).rejects.toThrow('Request failed: 500');
    });
  });

  describe('204 responses', () => {
    it('returns undefined for 204 No Content', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
        json: () => Promise.resolve(undefined),
      });

      const result = await acknowledgeWarning('some-id');
      expect(result).toBeUndefined();
    });
  });

  describe('Goals API', () => {
    it('createGoal sends POST with title and description', async () => {
      const goal = { id: '1', title: 'Ship MVP', status: 'Active' };
      mockFetch.mockResolvedValueOnce(mockResponse(goal));

      const result = await createGoal('Ship MVP', 'Launch in 90 days');

      expect(mockFetch).toHaveBeenCalledWith(
        '/api/goals',
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify({ title: 'Ship MVP', description: 'Launch in 90 days' }),
        })
      );
      expect(result).toEqual(goal);
    });

    it('completeGoal sends PUT to correct endpoint', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse({ id: '1', status: 'Completed' }));

      await completeGoal('abc-123');

      expect(mockFetch).toHaveBeenCalledWith(
        '/api/goals/abc-123/complete',
        expect.objectContaining({ method: 'PUT' })
      );
    });

    it('abandonGoal includes reason in body', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse({ id: '1', status: 'Abandoned' }));

      await abandonGoal('abc-123', 'Changed direction');

      expect(mockFetch).toHaveBeenCalledWith(
        '/api/goals/abc-123/abandon',
        expect.objectContaining({
          method: 'PUT',
          body: JSON.stringify({ reason: 'Changed direction' }),
        })
      );
    });
  });

  describe('Projects API', () => {
    it('createProject sends correct payload', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse({ id: '1', title: 'App' }));

      await createProject('App', 'Description', 'goal-id');

      expect(mockFetch).toHaveBeenCalledWith(
        '/api/projects',
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify({ title: 'App', description: 'Description', goalId: 'goal-id' }),
        })
      );
    });

    it('updateProjectStatus sends PUT with status', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse({ id: '1', status: 'Paused' }));

      await updateProjectStatus('proj-1', 'Paused');

      expect(mockFetch).toHaveBeenCalledWith(
        '/api/projects/proj-1/status',
        expect.objectContaining({
          method: 'PUT',
          body: JSON.stringify({ status: 'Paused' }),
        })
      );
    });

    it('submitProjectChange sends full payload', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse({ id: '1', status: 'Pending' }));

      await submitProjectChange('New App', 'New desc', 'Good reason here', 'old-id');

      expect(mockFetch).toHaveBeenCalledWith(
        '/api/projects/change-request',
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify({
            proposedProjectTitle: 'New App',
            proposedProjectDescription: 'New desc',
            justification: 'Good reason here',
            replaceProjectId: 'old-id',
          }),
        })
      );
    });
  });

  describe('Daily Logs API', () => {
    it('logToday sends POST with all fields', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse({ id: '1' }));

      await logToday({
        deepWorkMinutes: 120,
        gymCompleted: true,
        learningMinutes: 30,
        alcoholFree: true,
        notes: 'Good day',
      });

      expect(mockFetch).toHaveBeenCalledWith(
        '/api/daily-logs',
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify({
            deepWorkMinutes: 120,
            gymCompleted: true,
            learningMinutes: 30,
            alcoholFree: true,
            notes: 'Good day',
          }),
        })
      );
    });

    it('getLogs constructs query params', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse([]));

      await getLogs('2024-06-01', '2024-06-07');

      expect(mockFetch).toHaveBeenCalledWith(
        '/api/daily-logs?from=2024-06-01&to=2024-06-07',
        expect.anything()
      );
    });

    it('getLogs without params omits query string', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse([]));

      await getLogs();

      expect(mockFetch).toHaveBeenCalledWith(
        '/api/daily-logs?',
        expect.anything()
      );
    });
  });

  describe('Weekly Reviews API', () => {
    it('generateWeeklyReview sends POST', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse({ id: '1' }));

      await generateWeeklyReview();

      expect(mockFetch).toHaveBeenCalledWith(
        '/api/weekly-reviews/generate',
        expect.objectContaining({ method: 'POST' })
      );
    });

    it('getLatestReview sends GET', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse({ id: '1' }));

      await getLatestReview();

      expect(mockFetch).toHaveBeenCalledWith(
        '/api/weekly-reviews/latest',
        expect.anything()
      );
    });
  });

  describe('Dashboard API', () => {
    it('getDashboard sends GET', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse({
        activeGoal: null,
        activeProjects: [],
        streaks: [],
        activeWarnings: [],
        latestReview: null,
        todayLog: null,
        score: { overallPercentage: 0 },
      }));

      const result = await getDashboard();

      expect(mockFetch).toHaveBeenCalledWith(
        '/api/dashboard',
        expect.anything()
      );
      expect(result.activeProjects).toEqual([]);
    });
  });
});
