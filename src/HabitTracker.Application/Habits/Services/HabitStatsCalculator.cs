using System.Globalization;
using HabitTracker.Domain.Enums;

namespace HabitTracker.Application.Habits.Services;

public static class HabitStatsCalculator
{
    public static int CalculateStreak(HabitFrequency frequency, IEnumerable<DateOnly> completedDates, DateOnly today) =>
        frequency == HabitFrequency.Weekly
            ? CalculateWeeklyStreak(completedDates, today)
            : CalculateDailyStreak(completedDates, today);

    public static int CalculateMonthlyRate(HabitFrequency frequency, IEnumerable<DateOnly> completedDates, DateOnly today) =>
        frequency == HabitFrequency.Weekly
            ? CalculateWeeklyMonthlyRate(completedDates, today)
            : CalculateDailyMonthlyRate(completedDates, today);

    public static int CalculateDailyStreak(IEnumerable<DateOnly> completedDates, DateOnly today)
    {
        var dates = completedDates.ToHashSet();

        var cursor = dates.Contains(today) ? today : today.AddDays(-1);
        if (!dates.Contains(cursor))
        {
            return 0;
        }

        var streak = 0;
        while (dates.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    public static int CalculateWeeklyStreak(IEnumerable<DateOnly> completedDates, DateOnly today)
    {
        var weeksWithCompletion = completedDates.Select(GetIsoWeekKey).ToHashSet();

        var cursor = weeksWithCompletion.Contains(GetIsoWeekKey(today)) ? today : today.AddDays(-7);
        if (!weeksWithCompletion.Contains(GetIsoWeekKey(cursor)))
        {
            return 0;
        }

        var streak = 0;
        while (weeksWithCompletion.Contains(GetIsoWeekKey(cursor)))
        {
            streak++;
            cursor = cursor.AddDays(-7);
        }

        return streak;
    }

    public static int CalculateDailyMonthlyRate(IEnumerable<DateOnly> completedDates, DateOnly today)
    {
        var daysElapsed = today.Day;
        var completedThisMonth = CountCompletedThisMonth(completedDates, today);

        return (int)Math.Round(completedThisMonth / (double)daysElapsed * 100);
    }

    public static int CalculateWeeklyMonthlyRate(IEnumerable<DateOnly> completedDates, DateOnly today)
    {
        var weeksWithCompletion = completedDates.Select(GetIsoWeekKey).ToHashSet();

        var firstOfMonth = new DateOnly(today.Year, today.Month, 1);
        var elapsedWeeks = new HashSet<(int Year, int Week)>();
        for (var day = firstOfMonth; day <= today; day = day.AddDays(1))
        {
            elapsedWeeks.Add(GetIsoWeekKey(day));
        }

        var weeksCompleted = elapsedWeeks.Count(weeksWithCompletion.Contains);

        return (int)Math.Round(weeksCompleted / (double)elapsedWeeks.Count * 100);
    }

    public static int CountCompletedThisMonth(IEnumerable<DateOnly> completedDates, DateOnly today) =>
        completedDates.Count(d => d.Year == today.Year && d.Month == today.Month && d <= today);

    private static (int Year, int Week) GetIsoWeekKey(DateOnly date)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);
        return (ISOWeek.GetYear(dateTime), ISOWeek.GetWeekOfYear(dateTime));
    }
}
