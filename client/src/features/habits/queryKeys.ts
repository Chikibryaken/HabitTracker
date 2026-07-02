export const habitKeys = {
  all: ["habits"] as const,
  lists: () => [...habitKeys.all, "list"] as const,
  list: (includeArchived: boolean) => [...habitKeys.lists(), { includeArchived }] as const,
  details: () => [...habitKeys.all, "detail"] as const,
  detail: (id: string) => [...habitKeys.details(), id] as const,
  completions: (habitId: string, from: string, to: string) =>
    [...habitKeys.detail(habitId), "completions", { from, to }] as const,
  stats: (habitId: string, today: string) =>
    [...habitKeys.detail(habitId), "stats", { today }] as const,
};
