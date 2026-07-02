using HabitTracker.Application.Habits.Services;
using HabitTracker.Domain.Enums;

namespace HabitTracker.Tests.Habits;

public class HabitStatsCalculatorTests
{
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

        var streak = HabitStatsCalculator.CalculateStreak(HabitFrequency.Daily, dates, Today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void CalculateStreak_WeeklyFrequency_DispatchesToWeeklyAlgorithm()
    {
        var dates = new[] { Today, Today.AddDays(-7) };

        var streak = HabitStatsCalculator.CalculateStreak(HabitFrequency.Weekly, dates, Today);

        Assert.Equal(2, streak);
    }
}
