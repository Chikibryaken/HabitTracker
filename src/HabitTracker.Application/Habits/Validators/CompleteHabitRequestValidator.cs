using FluentValidation;
using HabitTracker.Application.Habits.Dtos;

namespace HabitTracker.Application.Habits.Validators;

public class CompleteHabitRequestValidator : AbstractValidator<CompleteHabitRequest>
{
    public CompleteHabitRequestValidator()
    {
        RuleFor(x => x.Date)
            .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1))
            .WithMessage("Date cannot be in the future.");
    }
}
