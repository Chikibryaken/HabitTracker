export type HabitFrequency = "Daily" | "Weekly" | "SpecificDays" | "EveryOtherDay";

export type DayOfWeek =
  | "Sunday"
  | "Monday"
  | "Tuesday"
  | "Wednesday"
  | "Thursday"
  | "Friday"
  | "Saturday";

export interface Habit {
  id: string;
  name: string;
  description?: string;
  frequency: HabitFrequency;
  daysOfWeek?: DayOfWeek[];
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
  daysOfWeek?: DayOfWeek[];
}

export interface UpdateHabitInput {
  name: string;
  description?: string;
  frequency: HabitFrequency;
  daysOfWeek?: DayOfWeek[];
}

export interface HabitStats {
  habitId: string;
  currentStreak: number;
  monthlyCompletionRate: number;
  frequency: HabitFrequency;
  completedThisMonth: number;
  daysElapsedThisMonth: number;
}
