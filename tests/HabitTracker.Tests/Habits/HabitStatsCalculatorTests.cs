using HabitTracker.Application.Habits.Services;
using HabitTracker.Domain.Enums;

namespace HabitTracker.Tests.Habits;

public class HabitStatsCalculatorTests
{
    // 2026-07-15 is a Wednesday. Reference dates below rely on this.
    private static readonly DateOnly Today = new(2026, 7, 15);

    [Fact]
    public void CalculateDailyStreak_EmptySet_ReturnsZero()
    {
        var streak = HabitStatsCalculator.CalculateDailyStreak([], Today);

        Assert.Equal(0, streak);
    }

    [Fact]
    public void CalculateDailyStreak_TodayAndYesterdayMarked_ReturnsTwo()
    {
        var dates = new[] { Today, Today.AddDays(-1) };

        var streak = HabitStatsCalculator.CalculateDailyStreak(dates, Today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void CalculateDailyStreak_GapTwoDaysAgo_BreaksStreak()
    {
        // Today and yesterday marked, day before yesterday missing.
        // A mark 3 days ago exists too, but the gap must stop the count from reaching it.
        var dates = new[] { Today, Today.AddDays(-1), Today.AddDays(-3) };

        var streak = HabitStatsCalculator.CalculateDailyStreak(dates, Today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void CalculateDailyStreak_TodayMissingYesterdayPresent_StreakNotBroken()
    {
        var dates = new[] { Today.AddDays(-1), Today.AddDays(-2) };

        var streak = HabitStatsCalculator.CalculateDailyStreak(dates, Today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void CalculateDailyStreak_TodayAndYesterdayBothMissing_ReturnsZero()
    {
        var dates = new[] { Today.AddDays(-2) };

        var streak = HabitStatsCalculator.CalculateDailyStreak(dates, Today);

        Assert.Equal(0, streak);
    }

    [Fact]
    public void CalculateDailyStreak_UnsortedDates_ComputesCorrectly()
    {
        var dates = new[]
        {
            Today.AddDays(-4),
            Today,
            Today.AddDays(-2),
            Today.AddDays(-1),
            Today.AddDays(-3),
        };

        var streak = HabitStatsCalculator.CalculateDailyStreak(dates, Today);

        Assert.Equal(5, streak);
    }

    [Fact]
    public void CalculateDailyMonthlyRate_SeveralDaysCompleted_DenominatorIsElapsedDays()
    {
        var today = new DateOnly(2026, 7, 15);
        var dates = new[]
        {
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 3),
            new DateOnly(2026, 7, 5),
            new DateOnly(2026, 7, 10),
            new DateOnly(2026, 7, 15),
        };

        var rate = HabitStatsCalculator.CalculateDailyMonthlyRate(dates, today);

        // 5 completed / 15 elapsed days = 33.33% -> rounds to 33
        Assert.Equal(33, rate);
    }

    [Fact]
    public void CalculateDailyMonthlyRate_IgnoresCompletionsFromOtherMonths()
    {
        var today = new DateOnly(2026, 7, 10);
        var dates = new[]
        {
            new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 5),
        };

        var rate = HabitStatsCalculator.CalculateDailyMonthlyRate(dates, today);

        // 2 completed / 10 elapsed days = 20%
        Assert.Equal(20, rate);
    }

    [Fact]
    public void CalculateDailyMonthlyRate_NoCompletions_ReturnsZero()
    {
        var today = new DateOnly(2026, 7, 10);

        var rate = HabitStatsCalculator.CalculateDailyMonthlyRate([], today);

        Assert.Equal(0, rate);
    }

    [Fact]
    public void CalculateWeeklyStreak_EmptySet_ReturnsZero()
    {
        var streak = HabitStatsCalculator.CalculateWeeklyStreak([], Today);

        Assert.Equal(0, streak);
    }

    [Fact]
    public void CalculateWeeklyStreak_CurrentAndPreviousWeekMarked_ReturnsTwo()
    {
        var dates = new[] { Today, Today.AddDays(-7) };

        var streak = HabitStatsCalculator.CalculateWeeklyStreak(dates, Today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void CalculateWeeklyStreak_CurrentWeekMissing_NotBroken()
    {
        var dates = new[] { Today.AddDays(-7), Today.AddDays(-14) };

        var streak = HabitStatsCalculator.CalculateWeeklyStreak(dates, Today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void CalculateWeeklyStreak_GapWeekBreaksStreak()
    {
        var dates = new[] { Today, Today.AddDays(-21) };

        var streak = HabitStatsCalculator.CalculateWeeklyStreak(dates, Today);

        Assert.Equal(1, streak);
    }

    [Fact]
    public void CountCompletedThisMonth_CountsOnlyCurrentMonthUpToToday()
    {
        var today = new DateOnly(2026, 7, 10);
        var dates = new[]
        {
            new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 10),
        };

        var count = HabitStatsCalculator.CountCompletedThisMonth(dates, today);

        Assert.Equal(2, count);
    }

    [Fact]
    public void CalculateStreak_DailyFrequency_DispatchesToDailyAlgorithm()
    {
        var dates = new[] { Today, Today.AddDays(-1) };

        var streak = HabitStatsCalculator.CalculateStreak(HabitFrequency.Daily, null, Today, dates, Today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void CalculateStreak_WeeklyFrequency_DispatchesToWeeklyAlgorithm()
    {
        var dates = new[] { Today, Today.AddDays(-7) };

        var streak = HabitStatsCalculator.CalculateStreak(HabitFrequency.Weekly, null, Today, dates, Today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void CalculateStreak_SpecificDaysFrequency_DispatchesToScheduledAlgorithm()
    {
        // Today (Wed 2026-07-15) and Mon 2026-07-13 are both scheduled (Mon/Wed/Fri) and completed.
        var mask = DaysOfWeekMask.Monday | DaysOfWeekMask.Wednesday | DaysOfWeekMask.Friday;
        var dates = new[] { Today, new DateOnly(2026, 7, 13) };

        var streak = HabitStatsCalculator.CalculateStreak(HabitFrequency.SpecificDays, mask, Today, dates, Today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void CalculateStreak_EveryOtherDayFrequency_DispatchesToScheduledAlgorithm()
    {
        // Habit created 2026-07-01 (Wed) -> scheduled every other day: 1, 3, 5, ... 15.
        var createdAt = new DateOnly(2026, 7, 1);
        var dates = new[] { Today, new DateOnly(2026, 7, 13) };

        var streak = HabitStatsCalculator.CalculateStreak(HabitFrequency.EveryOtherDay, null, createdAt, dates, Today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void CalculateSpecificDaysStreak_OnlySelectedWeekdaysCount_UnselectedDaysAreSkipped()
    {
        // Friday-only habit. Today is Wednesday (unscheduled) — it must have no effect either way.
        var mask = DaysOfWeekMask.Friday;
        var dates = new[] { new DateOnly(2026, 7, 10), new DateOnly(2026, 7, 3) }; // two consecutive Fridays

        var streak = HabitStatsCalculator.CalculateSpecificDaysStreak(dates, Today, mask);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void CalculateSpecificDaysStreak_TodayScheduledButIncomplete_GraceDoesNotBreakStreak()
    {
        var mask = DaysOfWeekMask.Monday | DaysOfWeekMask.Wednesday | DaysOfWeekMask.Friday;
        var dates = new[] { new DateOnly(2026, 7, 13) }; // last Monday, today (Wed) not done yet

        var streak = HabitStatsCalculator.CalculateSpecificDaysStreak(dates, Today, mask);

        Assert.Equal(1, streak);
    }

    [Fact]
    public void CalculateSpecificDaysStreak_EmptySet_ReturnsZero()
    {
        var mask = DaysOfWeekMask.Monday | DaysOfWeekMask.Wednesday | DaysOfWeekMask.Friday;

        var streak = HabitStatsCalculator.CalculateSpecificDaysStreak([], Today, mask);

        Assert.Equal(0, streak);
    }

    [Fact]
    public void CalculateSpecificDaysMonthlyRate_CountsOnlyScheduledDaysAsElapsed()
    {
        var mask = DaysOfWeekMask.Monday | DaysOfWeekMask.Wednesday | DaysOfWeekMask.Friday;
        // Scheduled Mon/Wed/Fri from 2026-07-01 through 2026-07-15: 1, 3, 6, 8, 10, 13, 15 = 7 days.
        var dates = new[]
        {
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 6),
            new DateOnly(2026, 7, 13),
        };

        var rate = HabitStatsCalculator.CalculateSpecificDaysMonthlyRate(dates, Today, mask);

        // 3 completed / 7 scheduled = 42.86% -> rounds to 43
        Assert.Equal(43, rate);
    }

    [Fact]
    public void CalculateEveryOtherDayStreak_ConsecutiveScheduledDaysCount()
    {
        var createdAt = new DateOnly(2026, 7, 1);
        var dates = new[] { Today, new DateOnly(2026, 7, 13) };

        var streak = HabitStatsCalculator.CalculateEveryOtherDayStreak(dates, Today, createdAt);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void CalculateEveryOtherDayStreak_TodayScheduledButIncomplete_GraceDoesNotBreakStreak()
    {
        var createdAt = new DateOnly(2026, 7, 1);
        var dates = new[] { new DateOnly(2026, 7, 13) };

        var streak = HabitStatsCalculator.CalculateEveryOtherDayStreak(dates, Today, createdAt);

        Assert.Equal(1, streak);
    }

    [Fact]
    public void CalculateEveryOtherDayMonthlyRate_CountsOnlyScheduledDaysAsElapsed()
    {
        var createdAt = new DateOnly(2026, 7, 1);
        // Scheduled every other day from creation through today: 1, 3, 5, 7, 9, 11, 13, 15 = 8 days.
        var dates = new[]
        {
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 5),
            new DateOnly(2026, 7, 9),
            new DateOnly(2026, 7, 13),
        };

        var rate = HabitStatsCalculator.CalculateEveryOtherDayMonthlyRate(dates, Today, createdAt);

        // 4 completed / 8 scheduled = 50%
        Assert.Equal(50, rate);
    }

    [Fact]
    public void CalculateMonthlyProgress_Weekly_StaysCalendarDayBased()
    {
        var today = new DateOnly(2026, 7, 10);
        var dates = new[] { new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 10) };

        var (completed, elapsed) = HabitStatsCalculator.CalculateMonthlyProgress(HabitFrequency.Weekly, null, today, dates, today);

        Assert.Equal(2, completed);
        Assert.Equal(10, elapsed);
    }

    [Fact]
    public void CalculateMonthlyProgress_SpecificDays_IsScheduledDayBased()
    {
        var mask = DaysOfWeekMask.Monday | DaysOfWeekMask.Wednesday | DaysOfWeekMask.Friday;
        var dates = new[] { new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 13) };

        var (completed, elapsed) = HabitStatsCalculator.CalculateMonthlyProgress(HabitFrequency.SpecificDays, mask, Today, dates, Today);

        Assert.Equal(2, completed);
        Assert.Equal(7, elapsed);
    }
}
