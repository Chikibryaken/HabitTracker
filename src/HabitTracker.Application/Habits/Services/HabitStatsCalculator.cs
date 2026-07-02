using System.Globalization;
using HabitTracker.Domain.Enums;

namespace HabitTracker.Application.Habits.Services;

public static class HabitStatsCalculator
{
    // Safety bound for the streak backward-walk: prevents an infinite loop if isScheduledDay
    // is (incorrectly) always false for a given predicate/date range.
    private const int MaxLookbackDays = 3650;

    public static int CalculateStreak(
        HabitFrequency frequency,
        DaysOfWeekMask? daysOfWeek,
        DateOnly habitCreatedAt,
        IEnumerable<DateOnly> completedDates,
        DateOnly today) =>
        frequency == HabitFrequency.Weekly
            ? CalculateWeeklyStreak(completedDates, today)
            : CalculateScheduledStreak(completedDates, today, date => IsScheduledDay(frequency, daysOfWeek, habitCreatedAt, date));

    public static int CalculateMonthlyRate(
        HabitFrequency frequency,
        DaysOfWeekMask? daysOfWeek,
        DateOnly habitCreatedAt,
        IEnumerable<DateOnly> completedDates,
        DateOnly today) =>
        frequency == HabitFrequency.Weekly
            ? CalculateWeeklyMonthlyRate(completedDates, today)
            : CalculateScheduledMonthlyRate(completedDates, today, date => IsScheduledDay(frequency, daysOfWeek, habitCreatedAt, date));

    // "Completed" / "elapsed" units for the response DTO. For Weekly this stays calendar-day
    // based (unchanged from v1); for the day-scheduled frequencies it's scheduled-day based,
    // e.g. a Mon/Wed/Fri habit reports completions out of elapsed Mon/Wed/Fri days, not all days.
    public static (int Completed, int Elapsed) CalculateMonthlyProgress(
        HabitFrequency frequency,
        DaysOfWeekMask? daysOfWeek,
        DateOnly habitCreatedAt,
        IEnumerable<DateOnly> completedDates,
        DateOnly today)
    {
        if (frequency == HabitFrequency.Weekly)
        {
            return (CountCompletedThisMonth(completedDates, today), today.Day);
        }

        return CalculateScheduledProgress(completedDates, today, date => IsScheduledDay(frequency, daysOfWeek, habitCreatedAt, date));
    }

    public static bool IsScheduledDay(HabitFrequency frequency, DaysOfWeekMask? daysOfWeek, DateOnly habitCreatedAt, DateOnly date) =>
        frequency switch
        {
            HabitFrequency.Daily => true,
            HabitFrequency.SpecificDays => (daysOfWeek ?? DaysOfWeekMask.None).HasFlag(date.DayOfWeek.ToDaysOfWeekMask()),
            HabitFrequency.EveryOtherDay => Math.Abs(date.DayNumber - habitCreatedAt.DayNumber) % 2 == 0,
            _ => throw new ArgumentOutOfRangeException(nameof(frequency), frequency, "Weekly habits use week-based scheduling, not day-based."),
        };

    public static int CalculateDailyStreak(IEnumerable<DateOnly> completedDates, DateOnly today) =>
        CalculateScheduledStreak(completedDates, today, _ => true);

    public static int CalculateDailyMonthlyRate(IEnumerable<DateOnly> completedDates, DateOnly today) =>
        CalculateScheduledMonthlyRate(completedDates, today, _ => true);

    public static int CalculateSpecificDaysStreak(IEnumerable<DateOnly> completedDates, DateOnly today, DaysOfWeekMask daysOfWeek) =>
        CalculateScheduledStreak(completedDates, today, date => daysOfWeek.HasFlag(date.DayOfWeek.ToDaysOfWeekMask()));

    public static int CalculateSpecificDaysMonthlyRate(IEnumerable<DateOnly> completedDates, DateOnly today, DaysOfWeekMask daysOfWeek) =>
        CalculateScheduledMonthlyRate(completedDates, today, date => daysOfWeek.HasFlag(date.DayOfWeek.ToDaysOfWeekMask()));

    public static int CalculateEveryOtherDayStreak(IEnumerable<DateOnly> completedDates, DateOnly today, DateOnly habitCreatedAt) =>
        CalculateScheduledStreak(completedDates, today, date => IsEveryOtherDayScheduled(habitCreatedAt, date));

    public static int CalculateEveryOtherDayMonthlyRate(IEnumerable<DateOnly> completedDates, DateOnly today, DateOnly habitCreatedAt) =>
        CalculateScheduledMonthlyRate(completedDates, today, date => IsEveryOtherDayScheduled(habitCreatedAt, date));

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

    // Shared primitive behind Daily/SpecificDays/EveryOtherDay streaks: walk backward from today,
    // skipping non-scheduled days (they neither count nor break the streak). Today gets a grace
    // period when it's scheduled but not yet completed, since the day isn't over.
    private static int CalculateScheduledStreak(IEnumerable<DateOnly> completedDates, DateOnly today, Func<DateOnly, bool> isScheduledDay)
    {
        var dates = completedDates.ToHashSet();
        var cursor = today;
        var streak = 0;

        while (today.DayNumber - cursor.DayNumber <= MaxLookbackDays)
        {
            if (isScheduledDay(cursor))
            {
                if (dates.Contains(cursor))
                {
                    streak++;
                }
                else if (cursor != today)
                {
                    break;
                }
            }

            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    // Shared primitive behind Daily/SpecificDays/EveryOtherDay monthly progress: counts scheduled
    // days from the 1st of the month through today, and how many of those are completed.
    private static (int Completed, int Elapsed) CalculateScheduledProgress(IEnumerable<DateOnly> completedDates, DateOnly today, Func<DateOnly, bool> isScheduledDay)
    {
        var dates = completedDates.ToHashSet();
        var firstOfMonth = new DateOnly(today.Year, today.Month, 1);

        var elapsed = 0;
        var completed = 0;

        for (var day = firstOfMonth; day <= today; day = day.AddDays(1))
        {
            if (!isScheduledDay(day))
            {
                continue;
            }

            elapsed++;
            if (dates.Contains(day))
            {
                completed++;
            }
        }

        return (completed, elapsed);
    }

    private static int CalculateScheduledMonthlyRate(IEnumerable<DateOnly> completedDates, DateOnly today, Func<DateOnly, bool> isScheduledDay)
    {
        var (completed, elapsed) = CalculateScheduledProgress(completedDates, today, isScheduledDay);

        return elapsed == 0 ? 0 : (int)Math.Round(completed / (double)elapsed * 100);
    }

    private static bool IsEveryOtherDayScheduled(DateOnly habitCreatedAt, DateOnly date) =>
        Math.Abs(date.DayNumber - habitCreatedAt.DayNumber) % 2 == 0;

    private static (int Year, int Week) GetIsoWeekKey(DateOnly date)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);
        return (ISOWeek.GetYear(dateTime), ISOWeek.GetWeekOfYear(dateTime));
    }
}
