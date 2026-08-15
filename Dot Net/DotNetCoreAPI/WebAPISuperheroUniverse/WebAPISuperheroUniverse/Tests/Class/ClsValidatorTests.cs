using WebAPISuperheroUniverse.API.Class;
using WebAPISuperheroUniverse.Entities.DTOs;

namespace WebAPISuperheroUniverse.Tests.Class;

public class ClsRegisterRequestValidatorTests
{
    private readonly ClsRegisterRequestValidator _validator = new();

    private static ModelRegisterRequest Valid(string? username = null, string? email = null, string? password = null) =>
        new(username ?? "validuser", email ?? "valid@example.com", password ?? "ValidPass1");

    [Fact]
    public void Accepts_AWellFormedRequest() =>
        Assert.True(_validator.Validate(Valid()).IsValid);

    [Theory]
    [InlineData("ab")]                    // too short
    [InlineData("")]                      // missing
    [InlineData("has space")]             // illegal character
    [InlineData("has@symbol")]            // illegal character
    public void Rejects_AnInvalidUsername(string username) =>
        Assert.False(_validator.Validate(Valid(username: username)).IsValid);

    [Theory]
    [InlineData("valid.user")]
    [InlineData("valid_user")]
    [InlineData("valid-user")]
    [InlineData("user123")]
    public void Accepts_TheDocumentedUsernameCharacters(string username) =>
        Assert.True(_validator.Validate(Valid(username: username)).IsValid);

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    [InlineData("@no-local-part.com")]
    public void Rejects_AnInvalidEmail(string email) =>
        Assert.False(_validator.Validate(Valid(email: email)).IsValid);

    [Fact]
    public void Accepts_ASingleLabelDomain()
    {
        // FluentValidation's EmailAddress() follows the spec rather than folk wisdom:
        // user@localhost is a legitimate address, so this is accepted by design.
        Assert.True(_validator.Validate(Valid(email: "user@localhost")).IsValid);
    }

    [Theory]
    [InlineData("Short1")]        // under 8 characters
    [InlineData("alllowercase1")] // no uppercase
    [InlineData("ALLUPPERCASE1")] // no lowercase
    [InlineData("NoDigitsHere")]  // no digit
    [InlineData("")]              // missing
    public void Rejects_AWeakPassword(string password) =>
        Assert.False(_validator.Validate(Valid(password: password)).IsValid);

    [Fact]
    public void ReportsTheOffendingField_SoTheClientCanHighlightIt()
    {
        var result = _validator.Validate(Valid(password: "weak"));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ModelRegisterRequest.Password));
    }
}

public class ClsSuperheroValidatorTests
{
    private readonly ClsSuperheroCreateRequestValidator _validator = new();

    private static ModelSuperheroCreateRequest Valid(
        string name = "Test Hero",
        string universe = "Marvel",
        string alignment = "Hero",
        int powerLevel = 50,
        int intelligence = 50) =>
        new(name, "Real Name", universe, alignment, "Description",
            powerLevel, intelligence, 50, 50, 50, 50, null);

    [Fact]
    public void Accepts_AWellFormedHero() =>
        Assert.True(_validator.Validate(Valid()).IsValid);

    [Theory]
    [InlineData("Hero")]
    [InlineData("Villain")]
    [InlineData("Anti-Hero")]
    public void Accepts_EveryAlignmentTheDatabaseAllows(string alignment) =>
        Assert.True(_validator.Validate(Valid(alignment: alignment)).IsValid);

    [Theory]
    [InlineData("Sidekick")]
    [InlineData("hero")]     // wrong casing - the CHECK constraint is case sensitive
    [InlineData("")]
    public void Rejects_AnAlignmentOutsideTheAllowedSet(string alignment) =>
        Assert.False(_validator.Validate(Valid(alignment: alignment)).IsValid);

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(500)]
    public void Rejects_AnAttributeOutsideZeroToOneHundred(int value)
    {
        // Mirrors the CK_xtSuperheroes_* constraints, so a bad value is a 400 rather than a 500
        // from a constraint violation.
        Assert.False(_validator.Validate(Valid(powerLevel: value)).IsValid);
        Assert.False(_validator.Validate(Valid(intelligence: value)).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Accepts_TheBoundaryValues(int value) =>
        Assert.True(_validator.Validate(Valid(powerLevel: value)).IsValid);

    [Fact]
    public void Rejects_AMissingName() =>
        Assert.False(_validator.Validate(Valid(name: "")).IsValid);

    [Fact]
    public void Accepts_ACustomUniverse() =>
        // The schema deliberately does not constrain Universe, so original settings are allowed.
        Assert.True(_validator.Validate(Valid(universe: "My Own Universe")).IsValid);
}

public class ClsBattleValidatorTests
{
    private readonly ClsSimulateBattleRequestValidator _validator = new();

    [Fact]
    public void Accepts_TwoDifferentHeroes() =>
        Assert.True(_validator.Validate(new ModelSimulateBattleRequest(1, 2)).IsValid);

    [Fact]
    public void Rejects_AHeroFightingItself()
    {
        var result = _validator.Validate(new ModelSimulateBattleRequest(5, 5));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("cannot battle themselves"));
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 0)]
    [InlineData(-1, 2)]
    public void Rejects_NonPositiveIds(int hero1, int hero2) =>
        Assert.False(_validator.Validate(new ModelSimulateBattleRequest(hero1, hero2)).IsValid);
}

public class ClsMissionValidatorTests
{
    private readonly ClsMissionRequestValidator _validator = new();

    private static ModelMissionRequest Valid(string difficulty = "Medium", int requiredHeroCount = 3) =>
        new("Test Mission", "Description", "Location", difficulty, requiredHeroCount);

    [Theory]
    [InlineData("Easy")]
    [InlineData("Medium")]
    [InlineData("Hard")]
    public void Accepts_EveryDifficultyTheDatabaseAllows(string difficulty) =>
        Assert.True(_validator.Validate(Valid(difficulty: difficulty)).IsValid);

    [Theory]
    [InlineData("Impossible")]
    [InlineData("easy")]
    [InlineData("")]
    public void Rejects_AnUnknownDifficulty(string difficulty) =>
        Assert.False(_validator.Validate(Valid(difficulty: difficulty)).IsValid);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public void Rejects_AnOutOfRangeHeroRequirement(int count) =>
        Assert.False(_validator.Validate(Valid(requiredHeroCount: count)).IsValid);

    [Fact]
    public void Accepts_ASingleHeroMission() =>
        Assert.True(_validator.Validate(Valid(requiredHeroCount: 1)).IsValid);

    [Fact]
    public void StartRequest_RejectsAnEmptySquad() =>
        Assert.False(new ClsStartMissionRequestValidator().Validate(new ModelStartMissionRequest([])).IsValid);

    [Fact]
    public void StartRequest_AcceptsASquad() =>
        Assert.True(new ClsStartMissionRequestValidator().Validate(new ModelStartMissionRequest([1, 2, 3])).IsValid);
}

