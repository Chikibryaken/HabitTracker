using FluentValidation;
using HabitTracker.Application.Habits.Dtos;

namespace HabitTracker.Application.Habits.Validators;

public class UpdateHabitRequestValidator : AbstractValidator<UpdateHabitRequest>
{
    public UpdateHabitRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Frequency).IsInEnum();
    }
}
