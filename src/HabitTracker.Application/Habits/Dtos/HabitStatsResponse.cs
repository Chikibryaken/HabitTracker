using HabitTracker.Domain.Enums;

namespace HabitTracker.Application.Habits.Dtos;

public record HabitStatsResponse(
    Guid HabitId,
    int CurrentStreak,
    int MonthlyCompletionRate,
    HabitFrequency Frequency,
    int CompletedThisMonth,
    int DaysElapsedThisMonth);
