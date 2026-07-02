using HabitTracker.Application.Habits.Dtos;
using HabitTracker.Domain.Entities;
using HabitTracker.Domain.Enums;

namespace HabitTracker.Application.Habits.Mapping;

public static class HabitMappingExtensions
{
    public static HabitResponse ToResponse(this Habit habit) =>
        new(
            habit.Id,
            habit.Name,
            habit.Description,
            habit.Frequency,
            habit.DaysOfWeek?.ToDayOfWeekList(),
            habit.CreatedAt,
            habit.IsArchived);

    public static CompletionResponse ToResponse(this HabitCompletion completion) =>
        new(completion.Date, completion.CreatedAt);
}
