using UnitConnect.Application.Abstractions;

namespace UnitConnect.Infrastructure.Services;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plaintext) =>
        BCrypt.Net.BCrypt.HashPassword(plaintext, workFactor: 12);

    public bool Verify(string plaintext, string hash) =>
        BCrypt.Net.BCrypt.Verify(plaintext, hash);
}
