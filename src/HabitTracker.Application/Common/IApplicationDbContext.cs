using HabitTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Application.Common;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Habit> Habits { get; }

    DbSet<HabitCompletion> HabitCompletions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
