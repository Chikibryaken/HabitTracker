using System.Security.Claims;
using FluentValidation;
using HabitTracker.Api.Extensions;
using HabitTracker.Application.Habits.Dtos;
using HabitTracker.Application.Habits.Interfaces;

namespace HabitTracker.Api.Endpoints;

public static class CompletionEndpoints
{
    private const int MaxRangeDays = 366;

    public static IEndpointRouteBuilder MapCompletionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/habits/{habitId:guid}").RequireAuthorization();

        group.MapPost("/complete", async (
            Guid habitId,
            CompleteHabitRequest request,
            IValidator<CompleteHabitRequest> validator,
            ClaimsPrincipal user,
            IHabitService habitService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var completed = await habitService.CompleteHabitAsync(userId.Value, habitId, request.Date, cancellationToken);
            return completed ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/complete", async (
            Guid habitId,
            DateOnly? date,
            ClaimsPrincipal user,
            IHabitService habitService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            if (date is null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["date"] = ["'date' query parameter is required."]
                });
            }

            var removed = await habitService.UncompleteHabitAsync(userId.Value, habitId, date.Value, cancellationToken);
            return removed ? Results.NoContent() : Results.NotFound();
        });

        group.MapGet("/completions", async (
            Guid habitId,
            DateOnly? from,
            DateOnly? to,
            ClaimsPrincipal user,
            IHabitService habitService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            if (from is null || to is null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["from"] = ["'from' and 'to' query parameters are required."]
                });
            }

            if (from.Value > to.Value)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["from"] = ["'from' must be less than or equal to 'to'."]
                });
            }

            if (to.Value.DayNumber - from.Value.DayNumber > MaxRangeDays)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["range"] = [$"Date range cannot exceed {MaxRangeDays} days."]
                });
            }

            var completions = await habitService.GetCompletionsAsync(userId.Value, habitId, from.Value, to.Value, cancellationToken);
            return completions is null ? Results.NotFound() : Results.Ok(completions);
        });

        return app;
    }
}
