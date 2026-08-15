namespace CommonAPI.Security;

public interface IPasswordHasher
{
    string Hash(string password);

    /// <summary>Returns false for a wrong password OR a malformed/corrupt stored hash - never throws.</summary>
    bool Verify(string password, string passwordHash);
}
