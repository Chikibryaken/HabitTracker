using FluentValidation;
using HabitTracker.Application.Auth.Interfaces;
using HabitTracker.Application.Auth.Services;
using HabitTracker.Application.Habits.Interfaces;
using HabitTracker.Application.Habits.Services;
using HabitTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IHabitService, HabitService>();

        return services;
    }
}
