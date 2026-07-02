namespace HabitTracker.Application.Auth.Interfaces;

public interface ITokenHasher
{
    string Hash(string rawToken);
}
