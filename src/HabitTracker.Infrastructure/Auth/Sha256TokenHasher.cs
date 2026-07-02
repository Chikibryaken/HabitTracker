using System.Security.Cryptography;
using System.Text;
using HabitTracker.Application.Auth.Interfaces;

namespace HabitTracker.Infrastructure.Auth;

public class Sha256TokenHasher : ITokenHasher
{
    public string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
