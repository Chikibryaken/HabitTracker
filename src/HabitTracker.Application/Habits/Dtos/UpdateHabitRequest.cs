using HabitTracker.Domain.Enums;

namespace HabitTracker.Application.Habits.Dtos;

public record UpdateHabitRequest(
    string Name,
    string? Description,
    HabitFrequency Frequency,
    IReadOnlyList<DayOfWeek>? DaysOfWeek);
