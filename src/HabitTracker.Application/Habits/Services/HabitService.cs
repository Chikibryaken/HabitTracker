using HabitTracker.Application.Common;
using HabitTracker.Application.Habits.Dtos;
using HabitTracker.Application.Habits.Interfaces;
using HabitTracker.Application.Habits.Mapping;
using HabitTracker.Domain.Entities;
using HabitTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Application.Habits.Services;

public class HabitService(IApplicationDbContext db) : IHabitService
{
    public async Task<IReadOnlyList<HabitResponse>> GetHabitsAsync(Guid userId, bool includeArchived, CancellationToken cancellationToken = default)
    {
        var query = db.Habits.Where(h => h.UserId == userId);

        if (!includeArchived)
        {
            query = query.Where(h => !h.IsArchived);
        }

        var habits = await query.OrderBy(h => h.CreatedAt).ToListAsync(cancellationToken);

        return habits.Select(h => h.ToResponse()).ToList();
    }

    public async Task<HabitResponse?> GetHabitByIdAsync(Guid userId, Guid habitId, CancellationToken cancellationToken = default)
    {
        var habit = await db.Habits.SingleOrDefaultAsync(h => h.Id == habitId && h.UserId == userId, cancellationToken);

        return habit?.ToResponse();
    }

    public async Task<HabitResponse> CreateHabitAsync(Guid userId, CreateHabitRequest request, CancellationToken cancellationToken = default)
    {
        var habit = new Habit
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            Frequency = request.Frequency,
            DaysOfWeek = request.Frequency == HabitFrequency.SpecificDays ? request.DaysOfWeek?.ToDaysOfWeekMask() : null,
            CreatedAt = DateTime.UtcNow,
            IsArchived = false
        };

        db.Habits.Add(habit);
        await db.SaveChangesAsync(cancellationToken);

        return habit.ToResponse();
    }

    public async Task<HabitResponse?> UpdateHabitAsync(Guid userId, Guid habitId, UpdateHabitRequest request, CancellationToken cancellationToken = default)
    {
        var habit = await db.Habits.SingleOrDefaultAsync(h => h.Id == habitId && h.UserId == userId, cancellationToken);
        if (habit is null)
        {
            return null;
        }

        habit.Name = request.Name;
        habit.Description = request.Description;
        habit.Frequency = request.Frequency;
        habit.DaysOfWeek = request.Frequency == HabitFrequency.SpecificDays ? request.DaysOfWeek?.ToDaysOfWeekMask() : null;

        await db.SaveChangesAsync(cancellationToken);

        return habit.ToResponse();
    }

    public async Task<bool> ArchiveHabitAsync(Guid userId, Guid habitId, CancellationToken cancellationToken = default)
    {
        var habit = await db.Habits.SingleOrDefaultAsync(h => h.Id == habitId && h.UserId == userId, cancellationToken);
        if (habit is null)
        {
            return false;
        }

        habit.IsArchived = true;
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteHabitAsync(Guid userId, Guid habitId, CancellationToken cancellationToken = default)
    {
        var habit = await db.Habits.SingleOrDefaultAsync(h => h.Id == habitId && h.UserId == userId, cancellationToken);
        if (habit is null)
        {
            return false;
        }

        db.Habits.Remove(habit);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CompleteHabitAsync(Guid userId, Guid habitId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var ownsHabit = await db.Habits.AnyAsync(h => h.Id == habitId && h.UserId == userId, cancellationToken);
        if (!ownsHabit)
        {
            return false;
        }

        var alreadyCompleted = await db.HabitCompletions.AnyAsync(c => c.HabitId == habitId && c.Date == date, cancellationToken);
        if (alreadyCompleted)
        {
            return true;
        }

        db.HabitCompletions.Add(new HabitCompletion
        {
            Id = Guid.NewGuid(),
            HabitId = habitId,
            Date = date,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UncompleteHabitAsync(Guid userId, Guid habitId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var ownsHabit = await db.Habits.AnyAsync(h => h.Id == habitId && h.UserId == userId, cancellationToken);
        if (!ownsHabit)
        {
            return false;
        }

        var completion = await db.HabitCompletions.SingleOrDefaultAsync(c => c.HabitId == habitId && c.Date == date, cancellationToken);
        if (completion is not null)
        {
            db.HabitCompletions.Remove(completion);
            await db.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<IReadOnlyList<CompletionResponse>?> GetCompletionsAsync(Guid userId, Guid habitId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var ownsHabit = await db.Habits.AnyAsync(h => h.Id == habitId && h.UserId == userId, cancellationToken);
        if (!ownsHabit)
        {
            return null;
        }

        var completions = await db.HabitCompletions
            .Where(c => c.HabitId == habitId && c.Date >= from && c.Date <= to)
            .OrderBy(c => c.Date)
            .ToListAsync(cancellationToken);

        return completions.Select(c => c.ToResponse()).ToList();
    }

    public async Task<HabitStatsResponse?> GetHabitStatsAsync(Guid userId, Guid habitId, DateOnly today, CancellationToken cancellationToken = default)
    {
        var habit = await db.Habits.SingleOrDefaultAsync(h => h.Id == habitId && h.UserId == userId, cancellationToken);
        if (habit is null)
        {
            return null;
        }

        var completedDates = await db.HabitCompletions
            .Where(c => c.HabitId == habitId)
            .Select(c => c.Date)
            .ToListAsync(cancellationToken);

        return BuildStats(habit, completedDates, today);
    }

    public async Task<DashboardStatsResponse> GetDashboardStatsAsync(Guid userId, DateOnly today, CancellationToken cancellationToken = default)
    {
        var habits = await db.Habits
            .Where(h => h.UserId == userId && !h.IsArchived)
            .ToListAsync(cancellationToken);

        var habitIds = habits.Select(h => h.Id).ToList();

        var completions = await db.HabitCompletions
            .Where(c => habitIds.Contains(c.HabitId))
            .Select(c => new { c.HabitId, c.Date })
            .ToListAsync(cancellationToken);

        var completionsByHabit = completions
            .GroupBy(c => c.HabitId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<DateOnly>)g.Select(c => c.Date).ToList());

        var stats = habits
            .Select(h => BuildStats(h, completionsByHabit.GetValueOrDefault(h.Id, []), today))
            .ToList();

        var averageRate = stats.Count > 0
            ? (int)Math.Round(stats.Average(s => s.MonthlyCompletionRate))
            : 0;

        return new DashboardStatsResponse(stats, stats.Count, averageRate);
    }

    private static HabitStatsResponse BuildStats(Habit habit, IReadOnlyList<DateOnly> completedDates, DateOnly today)
    {
        var habitCreatedAt = DateOnly.FromDateTime(habit.CreatedAt);

        var streak = HabitStatsCalculator.CalculateStreak(habit.Frequency, habit.DaysOfWeek, habitCreatedAt, completedDates, today);
        var monthlyRate = HabitStatsCalculator.CalculateMonthlyRate(habit.Frequency, habit.DaysOfWeek, habitCreatedAt, completedDates, today);
        var (completedThisMonth, daysElapsedThisMonth) = HabitStatsCalculator.CalculateMonthlyProgress(habit.Frequency, habit.DaysOfWeek, habitCreatedAt, completedDates, today);

        return new HabitStatsResponse(habit.Id, streak, monthlyRate, habit.Frequency, completedThisMonth, daysElapsedThisMonth);
    }
}
