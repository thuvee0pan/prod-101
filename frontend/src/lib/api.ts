import type {
  Dashboard,
  Goal,
  Project,
  DailyLog,
  Streak,
  Warning,
  WeeklyReview,
  ProjectChangeResponse,
  TodoItem,
} from '@/types/api';
import { logger } from '@/lib/logger';

const API_BASE = '/api';

function getToken(): string | null {
  if (typeof window === 'undefined') return null;
  try {
    const stored = localStorage.getItem('auth');
    if (!stored) return null;
    return JSON.parse(stored).token;
  } catch {
    return null;
  }
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const token = getToken();
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...options?.headers as Record<string, string>,
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  });

  if (res.status === 401) {
    logger.warn('API', 'Session expired — redirecting to sign-in', { path });
    localStorage.removeItem('auth');
    window.location.href = '/sign-in';
    throw new Error('Session expired. Please sign in again.');
  }

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    logger.error('API', `Request failed`, { method: options?.method ?? 'GET', path, status: res.status });
    throw new Error(body.error || `Request failed: ${res.status}`);
  }

  logger.debug('API', `${options?.method ?? 'GET'} ${path} → ${res.status}`);

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

// Todos
export const createTodo = (data: {
  title: string;
  description?: string;
  category: string;
  dueDate?: string;
}) => request<TodoItem>('/todos', { method: 'POST', body: JSON.stringify(data) });

export const getTodos = (category?: string, status?: string) => {
  const params = new URLSearchParams();
  if (category) params.set('category', category);
  if (status) params.set('status', status);
  const qs = params.toString();
  return request<TodoItem[]>(`/todos${qs ? `?${qs}` : ''}`);
};

export const getTodosByDate = (date: string) =>
  request<TodoItem[]>(`/todos/date/${date}`);

export const updateTodo = (id: string, data: {
  title?: string;
  description?: string;
  category?: string;
  status?: string;
  dueDate?: string;
}) => request<TodoItem>(`/todos/${id}`, { method: 'PUT', body: JSON.stringify(data) });

export const deleteTodo = (id: string) =>
  request<void>(`/todos/${id}`, { method: 'DELETE' });

// Dashboard
export const getDashboard = () => request<Dashboard>('/dashboard');
