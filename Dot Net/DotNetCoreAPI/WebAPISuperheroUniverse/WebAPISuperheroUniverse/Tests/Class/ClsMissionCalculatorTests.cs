using WebAPISuperheroUniverse.API.Class;
using WebAPISuperheroUniverse.Entities.Tables;

namespace WebAPISuperheroUniverse.Tests.Class;

public class ClsMissionCalculatorTests
{
    /// <summary>Roll 1 always succeeds unless the chance floor is somehow below 1.</summary>
    private static ClsMissionCalculator AlwaysLucky() => new(new ClsFixedRollProvider(1));

    /// <summary>Roll 100 fails unless the chance is 100, which the clamp forbids.</summary>
    private static ClsMissionCalculator AlwaysUnlucky() => new(new ClsFixedRollProvider(100));

    private static ClsMissionCalculator WithRoll(int roll) => new(new ClsFixedRollProvider(roll));

    private static List<ModelSuperhero> Squad(int count, int powerLevel) =>
        [.. Enumerable.Range(1, count).Select(i => ClsTestData.FlatHero(i, $"Hero {i}", powerLevel))];

    [Fact]
    public void Resolve_SucceedsOnTheLowestPossibleRoll()
    {
        var result = AlwaysLucky().Resolve(ClsTestData.Mission(), Squad(3, 65));

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Resolve_FailsOnTheHighestPossibleRoll()
    {
        // Even a perfect squad cannot reach 100%, so roll 100 must always fail.
        var result = AlwaysUnlucky().Resolve(ClsTestData.Mission(difficulty: "Easy", requiredHeroCount: 1), Squad(10, 100));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Resolve_GivesExactlyFiftyPercent_WhenSquadIsAtParAndCorrectlySized()
    {
        // Medium par is 65 and the squad is exactly the briefed size, so neither adjustment
        // applies and the base probability should come through untouched.
        var result = WithRoll(50).Resolve(ClsTestData.Mission(difficulty: "Medium", requiredHeroCount: 3), Squad(3, 65));

        Assert.Equal(50, result.SuccessChancePercent);
    }

    [Theory]
    [InlineData("Easy", 45)]
    [InlineData("Medium", 65)]
    [InlineData("Hard", 82)]
    public void Resolve_UsesADifferentParPerDifficulty(string difficulty, int par)
    {
        var result = WithRoll(50).Resolve(ClsTestData.Mission(difficulty: difficulty, requiredHeroCount: 2), Squad(2, par));

        Assert.Equal(50, result.SuccessChancePercent);
    }

    [Fact]
    public void Resolve_GivesTheSameSquadWorseOddsOnAHarderMission()
    {
        var squad = Squad(2, 70);

        var easy = WithRoll(50).Resolve(ClsTestData.Mission(1, difficulty: "Easy", requiredHeroCount: 2), squad);
        var medium = WithRoll(50).Resolve(ClsTestData.Mission(2, difficulty: "Medium", requiredHeroCount: 2), squad);
        var hard = WithRoll(50).Resolve(ClsTestData.Mission(3, difficulty: "Hard", requiredHeroCount: 2), squad);

        Assert.True(easy.SuccessChancePercent > medium.SuccessChancePercent);
        Assert.True(medium.SuccessChancePercent > hard.SuccessChancePercent);
    }

    [Fact]
    public void Resolve_PenalisesAnUnderstaffedSquad()
    {
        var mission = ClsTestData.Mission(difficulty: "Medium", requiredHeroCount: 4);

        var full = WithRoll(50).Resolve(mission, Squad(4, 65));
        var short1 = WithRoll(50).Resolve(mission, Squad(3, 65));
        var short2 = WithRoll(50).Resolve(mission, Squad(2, 65));

        Assert.True(full.SuccessChancePercent > short1.SuccessChancePercent);
        Assert.True(short1.SuccessChancePercent > short2.SuccessChancePercent);
    }

    [Fact]
    public void Resolve_RewardsReinforcements_ButWithADiminishingCap()
    {
        var mission = ClsTestData.Mission(difficulty: "Medium", requiredHeroCount: 2);

        var exact = WithRoll(50).Resolve(mission, Squad(2, 65));
        var plusTwo = WithRoll(50).Resolve(mission, Squad(4, 65));
        var plusTen = WithRoll(50).Resolve(mission, Squad(12, 65));

        Assert.True(plusTwo.SuccessChancePercent > exact.SuccessChancePercent);
        // The bonus is capped, so piling on more bodies stops helping.
        Assert.Equal(plusTwo.SuccessChancePercent + 5, plusTen.SuccessChancePercent);
    }

    [Fact]
    public void Resolve_RanksSquadsByAverageQuality_NotHeadcount()
    {
        // This is the design decision the whole algorithm turns on: a small elite squad should
        // beat a crowd of weak heroes, otherwise padding a roster would always be the best play.
        var mission = ClsTestData.Mission(difficulty: "Hard", requiredHeroCount: 2);

        var elite = WithRoll(50).Resolve(mission, Squad(2, 98));
        var crowd = WithRoll(50).Resolve(mission, Squad(8, 40));

        Assert.True(elite.SuccessChancePercent > crowd.SuccessChancePercent);
    }

    [Fact]
    public void Resolve_ClampsTheChanceBetweenFiveAndNinetyFive()
    {
        var hopeless = WithRoll(50).Resolve(
            ClsTestData.Mission(difficulty: "Hard", requiredHeroCount: 10), Squad(1, 0));
        var overwhelming = WithRoll(50).Resolve(
            ClsTestData.Mission(difficulty: "Easy", requiredHeroCount: 1), Squad(12, 100));

        Assert.InRange(hopeless.SuccessChancePercent, 5, 95);
        Assert.InRange(overwhelming.SuccessChancePercent, 5, 95);
        Assert.Equal(5, hopeless.SuccessChancePercent);
        Assert.Equal(95, overwhelming.SuccessChancePercent);
    }

    [Fact]
    public void Resolve_SucceedsExactlyWhenTheRollIsAtOrBelowTheChance()
    {
        var mission = ClsTestData.Mission(difficulty: "Medium", requiredHeroCount: 3);
        var squad = Squad(3, 65); // 50% chance

        Assert.True(WithRoll(50).Resolve(mission, squad).Succeeded, "roll == chance should succeed");
        Assert.False(WithRoll(51).Resolve(mission, squad).Succeeded, "roll just above chance should fail");
    }

    [Fact]
    public void Resolve_ReportsTheFactorsThatProducedTheChance()
    {
        var result = WithRoll(50).Resolve(ClsTestData.Mission(difficulty: "Hard", requiredHeroCount: 4), Squad(2, 90));

        Assert.Contains(result.Factors, f => f.Label == "Squad strength");
        Assert.Contains(result.Factors, f => f.Label == "Squad size");
        Assert.Contains(result.Factors, f => f.Label == "Base odds");

        // Understaffed by two, so the size factor must be negative and say so.
        var size = result.Factors.Single(f => f.Label == "Squad size");
        Assert.True(size.ContributionPercent < 0);
        Assert.Contains("understaffed", size.Detail);
    }

    [Fact]
    public void Resolve_SummaryReflectsTheOutcome()
    {
        var mission = ClsTestData.Mission(title: "Hold the Line", difficulty: "Easy", requiredHeroCount: 1);

        var won = AlwaysLucky().Resolve(mission, Squad(1, 90));
        var lost = AlwaysUnlucky().Resolve(mission, Squad(1, 90));

        Assert.Contains("Hold the Line", won.Summary);
        Assert.Contains("secured", won.Summary);
        Assert.Contains("not completed", lost.Summary);
    }

    [Fact]
    public void Resolve_TreatsAnUnknownDifficultyAsMedium()
    {
        // The database CHECK constraint prevents this, but the calculator should not explode if
        // it is ever handed unexpected data.
        var result = WithRoll(50).Resolve(ClsTestData.Mission(difficulty: "Nightmare", requiredHeroCount: 2), Squad(2, 65));

        Assert.Equal(50, result.SuccessChancePercent);
    }

    [Fact]
    public void Resolve_Throws_WhenTheSquadIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => WithRoll(50).Resolve(ClsTestData.Mission(), []));
    }

    [Fact]
    public void Resolve_Throws_WhenArgumentsAreNull()
    {
        Assert.Throws<ArgumentNullException>(() => WithRoll(50).Resolve(null!, Squad(1, 50)));
        Assert.Throws<ArgumentNullException>(() => WithRoll(50).Resolve(ClsTestData.Mission(), null!));
    }

    [Fact]
    public void ProductionRollProvider_StaysWithinOneToOneHundred()
    {
        var provider = new ClsMissionRollProvider();

        for (var i = 0; i < 2_000; i++)
        {
            Assert.InRange(provider.NextRoll(), 1, 100);
        }
    }
}
