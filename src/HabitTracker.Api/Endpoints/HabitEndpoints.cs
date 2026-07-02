using System.Security.Claims;
using FluentValidation;
using HabitTracker.Api.Extensions;
using HabitTracker.Application.Habits.Dtos;
using HabitTracker.Application.Habits.Interfaces;

namespace HabitTracker.Api.Endpoints;

public static class HabitEndpoints
{
    public static IEndpointRouteBuilder MapHabitEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/habits").RequireAuthorization();

        group.MapGet("/", async (
            ClaimsPrincipal user,
            IHabitService habitService,
            CancellationToken cancellationToken,
            bool includeArchived = false) =>
        {
            var userId = user.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var habits = await habitService.GetHabitsAsync(userId.Value, includeArchived, cancellationToken);
            return Results.Ok(habits);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            IHabitService habitService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var habit = await habitService.GetHabitByIdAsync(userId.Value, id, cancellationToken);
            return habit is null ? Results.NotFound() : Results.Ok(habit);
        });

        group.MapPost("/", async (
            CreateHabitRequest request,
            IValidator<CreateHabitRequest> validator,
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

            var created = await habitService.CreateHabitAsync(userId.Value, request, cancellationToken);
            return Results.Created($"/api/habits/{created.Id}", created);
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateHabitRequest request,
            IValidator<UpdateHabitRequest> validator,
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

            var updated = await habitService.UpdateHabitAsync(userId.Value, id, request, cancellationToken);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        group.MapPut("/{id:guid}/archive", async (
            Guid id,
            ClaimsPrincipal user,
            IHabitService habitService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var archived = await habitService.ArchiveHabitAsync(userId.Value, id, cancellationToken);
            return archived ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            IHabitService habitService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var deleted = await habitService.DeleteHabitAsync(userId.Value, id, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        group.MapGet("/{id:guid}/stats", async (
            Guid id,
            DateOnly? today,
            ClaimsPrincipal user,
            IHabitService habitService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var effectiveToday = today ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var stats = await habitService.GetHabitStatsAsync(userId.Value, id, effectiveToday, cancellationToken);
            return stats is null ? Results.NotFound() : Results.Ok(stats);
        });

        group.MapGet("/stats", async (
            DateOnly? today,
            ClaimsPrincipal user,
            IHabitService habitService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var effectiveToday = today ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var dashboard = await habitService.GetDashboardStatsAsync(userId.Value, effectiveToday, cancellationToken);
            return Results.Ok(dashboard);
        });

        return app;
    }
}
