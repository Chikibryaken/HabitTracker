import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { QueryKey } from "@tanstack/react-query";
import { completeHabit, getCompletions, uncompleteHabit } from "../../api/habits";
import type { Completion } from "../../types/habit";
import { formatLocalDate } from "./dateUtils";
import { habitKeys } from "./queryKeys";

export function useTodayCompletion(habitId: string) {
  const today = formatLocalDate(new Date());

  return useQuery({
    queryKey: habitKeys.completions(habitId, today, today),
    queryFn: () => getCompletions(habitId, today, today),
    select: (completions) => completions.length > 0,
  });
}

interface ToggleCompletionVariables {
  habitId: string;
  date: string;
  isCurrentlyCompleted: boolean;
}

interface CompletionsCacheEntry {
  queryKey: QueryKey;
  previousData: Completion[];
}

interface ToggleCompletionContext {
  entries: CompletionsCacheEntry[];
}

function completionsRangeOf(queryKey: QueryKey): { from: string; to: string } | null {
  const last = queryKey[queryKey.length - 1];
  if (last && typeof last === "object" && "from" in last && "to" in last) {
    return last as { from: string; to: string };
  }
  return null;
}

export function useToggleCompletion() {
  const queryClient = useQueryClient();

  return useMutation<void, Error, ToggleCompletionVariables, ToggleCompletionContext>({
    mutationFn: async ({ habitId, date, isCurrentlyCompleted }) => {
      if (isCurrentlyCompleted) {
        await uncompleteHabit(habitId, date);
      } else {
        await completeHabit(habitId, date);
      }
    },
    onMutate: async ({ habitId, date, isCurrentlyCompleted }) => {
      // Every cached completions query for this habit whose [from, to] range covers the
      // toggled date needs patching — not just the exact query that triggered the toggle.
      // This keeps the "today" checkbox, the month calendar, and any other open range in sync.
      const completionsPrefix = [...habitKeys.detail(habitId), "completions"];

      await queryClient.cancelQueries({ queryKey: completionsPrefix });

      const entries: CompletionsCacheEntry[] = [];

      for (const [queryKey, data] of queryClient.getQueriesData<Completion[]>({
        queryKey: completionsPrefix,
      })) {
        const range = completionsRangeOf(queryKey);
        if (!range || !data || date < range.from || date > range.to) {
          continue;
        }

        entries.push({ queryKey, previousData: data });

        const nextData = isCurrentlyCompleted
          ? data.filter((completion) => completion.date !== date)
          : [...data, { date, createdAt: new Date().toISOString() }];

        queryClient.setQueryData<Completion[]>(queryKey, nextData);
      }

      return { entries };
    },
    onError: (_error, _variables, context) => {
      context?.entries.forEach(({ queryKey, previousData }) => {
        queryClient.setQueryData(queryKey, previousData);
      });
    },
    onSettled: (_data, _error, variables) => {
      // Broad on purpose: a completion change can affect completions (any range), stats
      // (streak/rate), and the habit itself — invalidate everything under this habit.
      queryClient.invalidateQueries({ queryKey: habitKeys.detail(variables.habitId) });
    },
  });
}
