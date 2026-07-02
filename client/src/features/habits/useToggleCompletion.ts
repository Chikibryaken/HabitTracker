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

interface ToggleCompletionContext {
  queryKey: QueryKey;
  previousCompletions: Completion[] | undefined;
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
      const queryKey = habitKeys.completions(habitId, date, date);

      await queryClient.cancelQueries({ queryKey });

      const previousCompletions = queryClient.getQueryData<Completion[]>(queryKey);

      const optimisticCompletions: Completion[] = isCurrentlyCompleted
        ? []
        : [{ date, createdAt: new Date().toISOString() }];

      queryClient.setQueryData<Completion[]>(queryKey, optimisticCompletions);

      return { queryKey, previousCompletions };
    },
    onError: (_error, _variables, context) => {
      if (context) {
        queryClient.setQueryData(context.queryKey, context.previousCompletions);
      }
    },
    onSettled: (_data, _error, variables) => {
      const queryKey = habitKeys.completions(variables.habitId, variables.date, variables.date);
      queryClient.invalidateQueries({ queryKey });
    },
  });
}
