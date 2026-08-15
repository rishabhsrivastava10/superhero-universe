namespace CommonAPI.Security;

/// <summary>
/// BCrypt-based password hashing.
///
/// BCrypt is deliberately slow and salts every hash automatically, which is what makes it
/// suitable for passwords - unlike fast general-purpose hashes such as SHA-256/MD5, which can be
/// brute-forced at billions of guesses per second.
/// </summary>
public sealed class ClsPasswordHasher : IPasswordHasher
{
    // Work factor 12 - each increment doubles the hashing cost. High enough to be expensive to
    // attack, low enough to keep login responsive.
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Stored hash is malformed (e.g. legacy or corrupted data). Treat as a failed login
            // rather than letting an exception escape into the request pipeline.
            return false;
        }
    }
}
