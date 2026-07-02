import { useQuery } from "@tanstack/react-query";
import { getHabitStats } from "../../api/habits";
import { habitKeys } from "./queryKeys";

export function useHabitStats(habitId: string, today: string) {
  return useQuery({
    queryKey: habitKeys.stats(habitId, today),
    queryFn: () => getHabitStats(habitId, today),
  });
}
