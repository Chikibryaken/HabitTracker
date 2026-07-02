import { useQuery } from "@tanstack/react-query";
import { getHabit, getHabits } from "../../api/habits";
import { habitKeys } from "./queryKeys";

export function useHabits(includeArchived: boolean) {
  return useQuery({
    queryKey: habitKeys.list(includeArchived),
    queryFn: () => getHabits(includeArchived),
  });
}

export function useHabit(id: string) {
  return useQuery({
    queryKey: habitKeys.detail(id),
    queryFn: () => getHabit(id),
  });
}
