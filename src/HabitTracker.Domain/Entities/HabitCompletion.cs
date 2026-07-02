namespace HabitTracker.Domain.Entities;

public class HabitCompletion
{
    public Guid Id { get; set; }
    public Guid HabitId { get; set; }
    public DateOnly Date { get; set; }
    public DateTime CreatedAt { get; set; }

    public Habit Habit { get; set; } = null!;
}
