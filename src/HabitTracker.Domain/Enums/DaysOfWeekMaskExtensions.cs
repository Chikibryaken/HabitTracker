namespace HabitTracker.Domain.Enums;

public static class DaysOfWeekMaskExtensions
{
    public static DaysOfWeekMask ToDaysOfWeekMask(this DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Monday => DaysOfWeekMask.Monday,
        DayOfWeek.Tuesday => DaysOfWeekMask.Tuesday,
        DayOfWeek.Wednesday => DaysOfWeekMask.Wednesday,
        DayOfWeek.Thursday => DaysOfWeekMask.Thursday,
        DayOfWeek.Friday => DaysOfWeekMask.Friday,
        DayOfWeek.Saturday => DaysOfWeekMask.Saturday,
        DayOfWeek.Sunday => DaysOfWeekMask.Sunday,
        _ => DaysOfWeekMask.None,
    };

    public static DaysOfWeekMask ToDaysOfWeekMask(this IEnumerable<DayOfWeek> days)
    {
        var mask = DaysOfWeekMask.None;
        foreach (var day in days)
        {
            mask |= day.ToDaysOfWeekMask();
        }

        return mask;
    }

    public static IReadOnlyList<DayOfWeek> ToDayOfWeekList(this DaysOfWeekMask mask)
    {
        var days = new List<DayOfWeek>();
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            if (mask.HasFlag(day.ToDaysOfWeekMask()))
            {
                days.Add(day);
            }
        }

        return days;
    }
}
