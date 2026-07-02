using FluentValidation;
using HabitTracker.Application.Habits.Dtos;
using HabitTracker.Domain.Enums;

namespace HabitTracker.Application.Habits.Validators;

public class CreateHabitRequestValidator : AbstractValidator<CreateHabitRequest>
{
    public CreateHabitRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Frequency).IsInEnum();

        RuleFor(x => x.DaysOfWeek)
            .Must(days => days is { Count: > 0 })
            .When(x => x.Frequency == HabitFrequency.SpecificDays)
            .WithMessage("Select at least one day of the week for a specific-days habit.");
    }
}
