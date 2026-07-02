using HabitTracker.Application.Habits.Dtos;

namespace HabitTracker.Application.Habits.Interfaces;

public interface IHabitService
{
    Task<IReadOnlyList<HabitResponse>> GetHabitsAsync(Guid userId, bool includeArchived, CancellationToken cancellationToken = default);

    Task<HabitResponse?> GetHabitByIdAsync(Guid userId, Guid habitId, CancellationToken cancellationToken = default);

    Task<HabitResponse> CreateHabitAsync(Guid userId, CreateHabitRequest request, CancellationToken cancellationToken = default);

    Task<HabitResponse?> UpdateHabitAsync(Guid userId, Guid habitId, UpdateHabitRequest request, CancellationToken cancellationToken = default);

    Task<bool> ArchiveHabitAsync(Guid userId, Guid habitId, CancellationToken cancellationToken = default);

    Task<bool> DeleteHabitAsync(Guid userId, Guid habitId, CancellationToken cancellationToken = default);

    Task<bool> CompleteHabitAsync(Guid userId, Guid habitId, DateOnly date, CancellationToken cancellationToken = default);

    Task<bool> UncompleteHabitAsync(Guid userId, Guid habitId, DateOnly date, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CompletionResponse>?> GetCompletionsAsync(Guid userId, Guid habitId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<HabitStatsResponse?> GetHabitStatsAsync(Guid userId, Guid habitId, DateOnly today, CancellationToken cancellationToken = default);

    Task<DashboardStatsResponse> GetDashboardStatsAsync(Guid userId, DateOnly today, CancellationToken cancellationToken = default);
}
