import type {
  Dashboard,
  Goal,
  Project,
  DailyLog,
  Streak,
  Warning,
  WeeklyReview,
  ProjectChangeResponse,
} from '@/types/api';

const API_BASE = '/api';

// Temporary: hardcoded user ID for MVP. Replace with auth.
const USER_ID = '00000000-0000-0000-0000-000000000001';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      'X-User-Id': USER_ID,
      ...options?.headers,
    },
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body.error || `Request failed: ${res.status}`);
  }

  if (res.status === 204) return undefined as T;
  return res.json();
}

// Goals
export const createGoal = (title: string, description: string) =>
  request<Goal>('/goals', {
    method: 'POST',
    body: JSON.stringify({ title, description }),
  });

export const getActiveGoal = () => request<Goal>('/goals/active');

export const getAllGoals = () => request<Goal[]>('/goals');

export const completeGoal = (id: string) =>
  request<Goal>(`/goals/${id}/complete`, { method: 'PUT' });

export const abandonGoal = (id: string, reason: string) =>
  request<Goal>(`/goals/${id}/abandon`, {
    method: 'PUT',
    body: JSON.stringify({ reason }),
  });

// Projects
export const createProject = (title: string, description: string, goalId?: string) =>
  request<Project>('/projects', {
    method: 'POST',
    body: JSON.stringify({ title, description, goalId }),
  });

export const getActiveProjects = () => request<Project[]>('/projects/active');

export const getAllProjects = () => request<Project[]>('/projects');

export const updateProjectStatus = (id: string, status: string) =>
  request<Project>(`/projects/${id}/status`, {
    method: 'PUT',
    body: JSON.stringify({ status }),
  });

export const submitProjectChange = (
  proposedProjectTitle: string,
  proposedProjectDescription: string,
  justification: string,
  replaceProjectId: string
) =>
  request<ProjectChangeResponse>('/projects/change-request', {
    method: 'POST',
    body: JSON.stringify({
      proposedProjectTitle,
      proposedProjectDescription,
      justification,
      replaceProjectId,
    }),
  });

export const approveProjectChange = (id: string) =>
  request<Project>(`/projects/change-request/${id}/approve`, { method: 'POST' });

// Daily Logs
export const logToday = (data: {
  deepWorkMinutes: number;
  gymCompleted: boolean;
  learningMinutes: number;
  alcoholFree: boolean;
  notes?: string;
}) => request<DailyLog>('/daily-logs', { method: 'POST', body: JSON.stringify(data) });

export const getTodayLog = () => request<DailyLog>('/daily-logs/today');

export const getLogs = (from?: string, to?: string) => {
  const params = new URLSearchParams();
  if (from) params.set('from', from);
  if (to) params.set('to', to);
  return request<DailyLog[]>(`/daily-logs?${params}`);
};

export const getStreaks = () => request<Streak[]>('/daily-logs/streaks');

// Warnings
export const getWarnings = () => request<Warning[]>('/warnings');

export const acknowledgeWarning = (id: string) =>
  request<void>(`/warnings/${id}/acknowledge`, { method: 'PUT' });

// Weekly Reviews
export const generateWeeklyReview = () =>
  request<WeeklyReview>('/weekly-reviews/generate', { method: 'POST' });

export const getAllReviews = () => request<WeeklyReview[]>('/weekly-reviews');

export const getLatestReview = () => request<WeeklyReview>('/weekly-reviews/latest');

// Dashboard
export const getDashboard = () => request<Dashboard>('/dashboard');
