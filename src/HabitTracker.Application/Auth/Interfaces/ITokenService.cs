using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.Auth.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);

    string GenerateRefreshToken();
}
