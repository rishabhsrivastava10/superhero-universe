using CommonAPI.Security;

namespace WebAPISuperheroUniverse.Tests.Security;

public class ClsPasswordHasherTests
{
    private readonly ClsPasswordHasher _hasher = new();

    [Fact]
    public void Hash_ThenVerify_AcceptsTheCorrectPassword()
    {
        var hash = _hasher.Hash("CorrectHorse1");

        Assert.True(_hasher.Verify("CorrectHorse1", hash));
    }

    [Fact]
    public void Verify_RejectsTheWrongPassword()
    {
        var hash = _hasher.Hash("CorrectHorse1");

        Assert.False(_hasher.Verify("WrongHorse1", hash));
    }

    [Fact]
    public void Verify_IsCaseSensitive()
    {
        var hash = _hasher.Hash("CorrectHorse1");

        Assert.False(_hasher.Verify("correcthorse1", hash));
    }

    [Fact]
    public void Hash_NeverStoresThePasswordInPlainText()
    {
        const string password = "CorrectHorse1";

        var hash = _hasher.Hash(password);

        Assert.DoesNotContain(password, hash);
    }

    [Fact]
    public void Hash_ProducesADifferentHashEachTime_BecauseOfTheSalt()
    {
        // Two users with the same password must not share a hash, otherwise a single cracked
        // hash would expose every account using that password.
        var first = _hasher.Hash("SamePassword1");
        var second = _hasher.Hash("SamePassword1");

        Assert.NotEqual(first, second);
        Assert.True(_hasher.Verify("SamePassword1", first));
        Assert.True(_hasher.Verify("SamePassword1", second));
    }

    [Fact]
    public void Hash_UsesBCryptWithTheExpectedWorkFactor()
    {
        var hash = _hasher.Hash("Whatever1");

        // BCrypt hashes are self-describing: $2a$12$ means BCrypt, work factor 12.
        Assert.StartsWith("$2a$12$", hash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Hash_Throws_ForAnEmptyPassword(string? password)
    {
        Assert.ThrowsAny<ArgumentException>(() => _hasher.Hash(password!));
    }

    [Theory]
    [InlineData("not-a-bcrypt-hash")]
    [InlineData("")]
    [InlineData(null)]
    public void Verify_ReturnsFalseRatherThanThrowing_ForACorruptStoredHash(string? storedHash)
    {
        // A malformed row in the database must fail the login, not surface as a 500.
        Assert.False(_hasher.Verify("AnyPassword1", storedHash!));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForAnEmptySuppliedPassword()
    {
        var hash = _hasher.Hash("RealPassword1");

        Assert.False(_hasher.Verify("", hash));
        Assert.False(_hasher.Verify("   ", hash));
    }
}
