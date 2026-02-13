export interface Goal {
  id: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  status: string;
  daysRemaining: number;
  daysElapsed: number;
  createdAt: string;
}

export interface Project {
  id: string;
  title: string;
  description: string;
  status: string;
  goalId: string | null;
  createdAt: string;
}

export interface DailyLog {
  id: string;
  logDate: string;
  deepWorkMinutes: number;
  gymCompleted: boolean;
  learningMinutes: number;
  alcoholFree: boolean;
  notes: string | null;
  createdAt: string;
}

export interface Streak {
  streakType: string;
  currentCount: number;
  longestCount: number;
  lastLoggedDate: string;
}

export interface Warning {
  id: string;
  warningType: string;
  message: string;
  triggeredAt: string;
  acknowledged: boolean;
}

export interface WeeklyReview {
  id: string;
  weekStart: string;
  weekEnd: string;
  whatWorked: string;
  whereAvoided: string;
  whatToCut: string;
  aiSummary: string;
  generatedAt: string;
}

export interface ExecutionScore {
  weeklyDeepWorkMinutes: number;
  weeklyGymDays: number;
  weeklyLearningMinutes: number;
  weeklySoberDays: number;
  overallPercentage: number;
}

export interface Dashboard {
  activeGoal: Goal | null;
  activeProjects: Project[];
  streaks: Streak[];
  activeWarnings: Warning[];
  latestReview: WeeklyReview | null;
  todayLog: DailyLog | null;
  score: ExecutionScore;
}

export interface TodoItem {
  id: string;
  title: string;
  description: string | null;
  category: string;
  status: string;
  dueDate: string;
  sortOrder: number;
  createdAt: string;
  updatedAt: string;
}

export interface ProjectChangeResponse {
  id: string;
  proposedProjectTitle: string;
  justification: string;
  status: string;
  aiRecommendation: string | null;
  createdAt: string;
}
