namespace HabitTracker.Application.Habits.Dtos;

public record DashboardStatsResponse(
    IReadOnlyList<HabitStatsResponse> Habits,
    int TotalHabits,
    int AverageCompletionRate);
