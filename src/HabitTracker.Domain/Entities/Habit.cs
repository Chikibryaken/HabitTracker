using HabitTracker.Domain.Enums;

namespace HabitTracker.Domain.Entities;

public class Habit
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public HabitFrequency Frequency { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsArchived { get; set; }

    public User User { get; set; } = null!;
    public ICollection<HabitCompletion> Completions { get; set; } = new List<HabitCompletion>();
}
