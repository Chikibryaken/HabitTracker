using System.Security.Claims;
using FluentValidation;
using HabitTracker.Api.Extensions;
using HabitTracker.Application.Auth.Dtos;
using HabitTracker.Application.Auth.Exceptions;
using HabitTracker.Application.Auth.Interfaces;

namespace HabitTracker.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            IValidator<RegisterRequest> validator,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            try
            {
                var response = await authService.RegisterAsync(request, cancellationToken);
                return Results.Ok(response);
            }
            catch (EmailAlreadyExistsException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        group.MapPost("/login", async (
            LoginRequest request,
            IValidator<LoginRequest> validator,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            try
            {
                var response = await authService.LoginAsync(request, cancellationToken);
                return Results.Ok(response);
            }
            catch (InvalidCredentialsException)
            {
                return Results.Unauthorized();
            }
        });

        group.MapPost("/refresh", async (
            RefreshRequest request,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await authService.RefreshAsync(request, cancellationToken);
                return Results.Ok(response);
            }
            catch (InvalidRefreshTokenException)
            {
                return Results.Unauthorized();
            }
        });

        group.MapPost("/logout", async (
            RefreshRequest request,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await authService.LogoutAsync(request, cancellationToken);
                return Results.NoContent();
            }
            catch (InvalidRefreshTokenException)
            {
                return Results.Unauthorized();
            }
        }).RequireAuthorization();

        group.MapGet("/me", async (
            ClaimsPrincipal user,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var profile = await authService.GetProfileAsync(userId.Value, cancellationToken);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }).RequireAuthorization();

        return app;
    }
}
