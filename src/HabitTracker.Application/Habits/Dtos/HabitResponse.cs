using HabitTracker.Domain.Enums;

namespace HabitTracker.Application.Habits.Dtos;

public record HabitResponse(
    Guid Id,
    string Name,
    string? Description,
    HabitFrequency Frequency,
    IReadOnlyList<DayOfWeek>? DaysOfWeek,
    DateTime CreatedAt,
    bool IsArchived);
