import { apiClient } from "./client";
import type { Completion, CreateHabitInput, Habit, HabitStats, UpdateHabitInput } from "../types/habit";

export async function getHabits(includeArchived: boolean): Promise<Habit[]> {
  const response = await apiClient.get<Habit[]>("/habits", {
    params: { includeArchived },
  });
  return response.data;
}

export async function getHabit(id: string): Promise<Habit> {
  const response = await apiClient.get<Habit>(`/habits/${id}`);
  return response.data;
}

export async function createHabit(input: CreateHabitInput): Promise<Habit> {
  const response = await apiClient.post<Habit>("/habits", input);
  return response.data;
}

export async function updateHabit(id: string, input: UpdateHabitInput): Promise<Habit> {
  const response = await apiClient.put<Habit>(`/habits/${id}`, input);
  return response.data;
}

export async function archiveHabit(id: string): Promise<void> {
  await apiClient.put(`/habits/${id}/archive`);
}

export async function deleteHabit(id: string): Promise<void> {
  await apiClient.delete(`/habits/${id}`);
}

export async function completeHabit(habitId: string, date: string): Promise<void> {
  await apiClient.post(`/habits/${habitId}/complete`, { date });
}

export async function uncompleteHabit(habitId: string, date: string): Promise<void> {
  await apiClient.delete(`/habits/${habitId}/complete`, { params: { date } });
}

export async function getCompletions(habitId: string, from: string, to: string): Promise<Completion[]> {
  const response = await apiClient.get<Completion[]>(`/habits/${habitId}/completions`, {
    params: { from, to },
  });
  return response.data;
}

export async function getHabitStats(habitId: string, today: string): Promise<HabitStats> {
  const response = await apiClient.get<HabitStats>(`/habits/${habitId}/stats`, {
    params: { today },
  });
  return response.data;
}
