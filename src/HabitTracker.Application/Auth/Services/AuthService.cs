using HabitTracker.Application.Auth.Dtos;
using HabitTracker.Application.Auth.Exceptions;
using HabitTracker.Application.Auth.Interfaces;
using HabitTracker.Application.Common;
using HabitTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HabitTracker.Application.Auth.Services;

public class AuthService(
    IApplicationDbContext db,
    IPasswordHasher<User> passwordHasher,
    ITokenService tokenService,
    ITokenHasher tokenHasher,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var emailExists = await db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (emailExists)
        {
            throw new EmailAlreadyExistsException(request.Email);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            throw new InvalidCredentialsException();
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await FindActiveRefreshTokenAsync(request.RefreshToken, cancellationToken);

        existing.RevokedAt = DateTime.UtcNow;

        return await IssueTokensAsync(existing.User, cancellationToken);
    }

    public async Task LogoutAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await FindActiveRefreshTokenAsync(request.RefreshToken, cancellationToken);

        existing.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var habitCount = await db.Habits.CountAsync(h => h.UserId == userId && !h.IsArchived, cancellationToken);

        return new UserProfileResponse(user.Id, user.Email, user.CreatedAt, habitCount);
    }

    private async Task<RefreshToken> FindActiveRefreshTokenAsync(string rawRefreshToken, CancellationToken cancellationToken)
    {
        var tokenHash = tokenHasher.Hash(rawRefreshToken);

        var refreshToken = await db.RefreshTokens
            .Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (refreshToken is null || refreshToken.RevokedAt is not null || refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidRefreshTokenException();
        }

        return refreshToken;
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
        var rawRefreshToken = tokenService.GenerateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHasher.Hash(rawRefreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays)
        });

        await db.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, rawRefreshToken, expiresAt);
    }
}
