using HabitTracker.Domain.Enums;

namespace HabitTracker.Application.Habits.Dtos;

public record CreateHabitRequest(string Name, string? Description, HabitFrequency Frequency);
