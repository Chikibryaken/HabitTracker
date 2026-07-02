import { useQuery } from "@tanstack/react-query";
import { getCompletions } from "../../api/habits";
import { habitKeys } from "./queryKeys";

export function useHabitCompletions(habitId: string, from: string, to: string) {
  return useQuery({
    queryKey: habitKeys.completions(habitId, from, to),
    queryFn: () => getCompletions(habitId, from, to),
  });
}
