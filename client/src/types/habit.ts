export type HabitFrequency = "Daily" | "Weekly";

export interface Habit {
  id: string;
  name: string;
  description?: string;
  frequency: HabitFrequency;
  createdAt: string;
  isArchived: boolean;
}

export interface Completion {
  date: string;
  createdAt: string;
}

export interface CreateHabitInput {
  name: string;
  description?: string;
  frequency: HabitFrequency;
}

export interface UpdateHabitInput {
  name: string;
  description?: string;
  frequency: HabitFrequency;
}

export interface HabitStats {
  habitId: string;
  currentStreak: number;
  monthlyCompletionRate: number;
  frequency: HabitFrequency;
  completedThisMonth: number;
  daysElapsedThisMonth: number;
}
