namespace HabitTracker.Application.Auth.Dtos;

public record UserProfileResponse(Guid Id, string Email, DateTime CreatedAt, int HabitCount);
