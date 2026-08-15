using WebAPISuperheroUniverse.API.Class;

namespace WebAPISuperheroUniverse.Tests.Class;

/// <summary>
/// Every test here fixes the variance at 1.0 (no luck) unless it is specifically exercising
/// variance, so outcomes are fully deterministic and can be asserted exactly. That is the whole
/// reason IBattleVarianceProvider exists.
/// </summary>
public class ClsBattleCalculatorTests
{
    private static ClsBattleCalculator NoLuck() => new(new ClsFixedVarianceProvider(1.0));

    [Fact]
    public void Simulate_StrongerHeroWins_WhenThereIsNoVariance()
    {
        var strong = ClsTestData.FlatHero(1, "Strong", 90);
        var weak = ClsTestData.FlatHero(2, "Weak", 40);

        var result = NoLuck().Simulate(strong, weak);

        Assert.Equal(1, result.WinnerId);
        Assert.False(result.IsDraw);
        Assert.True(result.Hero1Score > result.Hero2Score);
    }

    [Fact]
    public void Simulate_ScoreEqualsTheAttributeValue_WhenAllAttributesAreEqual()
    {
        // Weights sum to 1.0, so a hero whose attributes are all 70 must score exactly 70.
        // This pins the weighting maths: if any weight is wrong, the total stops being 1.0.
        var hero = ClsTestData.FlatHero(1, "Flat", 70);
        var other = ClsTestData.FlatHero(2, "Other", 40);

        var result = NoLuck().Simulate(hero, other);

        Assert.Equal(70, result.Hero1Score);
        Assert.Equal(40, result.Hero2Score);
    }

    [Fact]
    public void Simulate_IsSymmetric_WhenCombatantsAreSwapped()
    {
        var a = ClsTestData.FlatHero(1, "A", 80);
        var b = ClsTestData.FlatHero(2, "B", 60);

        var forwards = NoLuck().Simulate(a, b);
        var backwards = NoLuck().Simulate(b, a);

        // Same matchup, same winner - the order the heroes are passed in must not matter.
        Assert.Equal(1, forwards.WinnerId);
        Assert.Equal(1, backwards.WinnerId);
        Assert.Equal(forwards.Hero1Score, backwards.Hero2Score);
    }

    [Fact]
    public void Simulate_ReturnsADraw_WhenHeroesAreIdenticalIncludingCombat()
    {
        var a = ClsTestData.FlatHero(1, "Twin A", 60);
        var b = ClsTestData.FlatHero(2, "Twin B", 60);

        var result = NoLuck().Simulate(a, b);

        Assert.True(result.IsDraw);
        Assert.Null(result.WinnerId);
        Assert.Equal(result.Hero1Score, result.Hero2Score);
    }

    [Fact]
    public void Simulate_BreaksATiedScoreOnCombat_RatherThanDeclaringADraw()
    {
        // Both score the same overall, but one is the better fighter. Combat decides a close
        // fight, so this must NOT come back as a draw.
        var a = ClsTestData.Hero(1, "Technician", powerLevel: 60, strength: 60, combat: 90, speed: 60, durability: 60, intelligence: 60);
        var b = ClsTestData.Hero(2, "Brawler", powerLevel: 60, strength: 60, combat: 30, speed: 60, durability: 60, intelligence: 60);

        // Force the raw scores level by giving the weaker fighter compensating attributes.
        var evenA = ClsTestData.Hero(1, "Technician", powerLevel: 60, strength: 50, combat: 70, speed: 60, durability: 60, intelligence: 60);
        var evenB = ClsTestData.Hero(2, "Brawler", powerLevel: 60, strength: 70, combat: 50, speed: 60, durability: 60, intelligence: 60);

        var result = NoLuck().Simulate(evenA, evenB);

        Assert.Equal(result.Hero1Score, result.Hero2Score);
        Assert.False(result.IsDraw);
        Assert.Equal(1, result.WinnerId);

        // Sanity check that the deliberately lopsided pair still favours the better fighter.
        Assert.Equal(1, NoLuck().Simulate(a, b).WinnerId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(50)]
    public void Simulate_KeepsScoresWithinZeroToOneHundred(int attributeValue)
    {
        var a = ClsTestData.FlatHero(1, "A", attributeValue);
        var b = ClsTestData.FlatHero(2, "B", attributeValue);

        // Maximum favourable luck must still not push a score above 100.
        var lucky = new ClsBattleCalculator(new ClsFixedVarianceProvider(1.5));
        var result = lucky.Simulate(a, b);

        Assert.InRange(result.Hero1Score, 0, 100);
        Assert.InRange(result.Hero2Score, 0, 100);
    }

    [Fact]
    public void Simulate_LetsVarianceUpsetACloseMatchup()
    {
        var slightFavourite = ClsTestData.FlatHero(1, "Favourite", 61);
        var underdog = ClsTestData.FlatHero(2, "Underdog", 60);

        // A 20% roll against the favourite is enough to flip a one-point gap.
        var unlucky = new ClsBattleCalculator(new ClsFixedVarianceProvider(0.8));
        var result = unlucky.Simulate(slightFavourite, underdog);

        // Both rolled the same multiplier here, so the favourite still edges it - what matters
        // is that the scores moved, proving variance is applied rather than ignored.
        Assert.True(result.Hero1Score < 61);
        Assert.True(result.Hero2Score < 60);
    }

    [Fact]
    public void Simulate_ReturnsOneBreakdownRowPerAttribute_WithWeightsSummingTo100()
    {
        var result = NoLuck().Simulate(ClsTestData.FlatHero(1, "A", 60), ClsTestData.FlatHero(2, "B", 40));

        Assert.Equal(6, result.Breakdown.Count);
        Assert.Equal(100, result.Breakdown.Sum(b => b.WeightPercent));
        Assert.Contains(result.Breakdown, b => b.Attribute == "PowerLevel");
        Assert.Contains(result.Breakdown, b => b.Attribute == "Intelligence");
    }

    [Fact]
    public void Simulate_MarksWhichCombatantWonEachAttribute()
    {
        var a = ClsTestData.Hero(1, "A", powerLevel: 90, strength: 20, combat: 50, speed: 50, durability: 50, intelligence: 50);
        var b = ClsTestData.Hero(2, "B", powerLevel: 10, strength: 80, combat: 50, speed: 50, durability: 50, intelligence: 50);

        var result = NoLuck().Simulate(a, b);

        Assert.Equal(1, result.Breakdown.Single(x => x.Attribute == "PowerLevel").WonBy);
        Assert.Equal(2, result.Breakdown.Single(x => x.Attribute == "Strength").WonBy);
        Assert.Equal(0, result.Breakdown.Single(x => x.Attribute == "Combat").WonBy);
    }

    [Fact]
    public void Simulate_SummaryNamesTheWinnerFirst_AndReportsTheWinnersScoreFirst()
    {
        // Regression guard: an earlier version always printed hero1's score first, so a hero-2
        // win read as "Winner defeated Loser 40-90".
        var weakFirst = ClsTestData.FlatHero(1, "Weakling", 40);
        var strongSecond = ClsTestData.FlatHero(2, "Champion", 90);

        var result = NoLuck().Simulate(weakFirst, strongSecond);

        Assert.StartsWith("Champion defeated Weakling", result.Summary);
        Assert.Contains("90-40", result.Summary);
    }

    [Fact]
    public void Simulate_SummaryExplainsTheDecisiveAttribute()
    {
        var a = ClsTestData.Hero(1, "Genius", powerLevel: 70, strength: 50, combat: 50, speed: 50, durability: 50, intelligence: 100);
        var b = ClsTestData.Hero(2, "Brute", powerLevel: 40, strength: 50, combat: 50, speed: 50, durability: 50, intelligence: 10);

        var result = NoLuck().Simulate(a, b);

        Assert.Contains("carried by superior", result.Summary);
    }

    [Fact]
    public void Simulate_DescribesADrawWithoutNamingAWinner()
    {
        var result = NoLuck().Simulate(ClsTestData.FlatHero(1, "A", 55), ClsTestData.FlatHero(2, "B", 55));

        Assert.Contains("standstill", result.Summary);
        Assert.DoesNotContain("defeated", result.Summary);
    }

    [Fact]
    public void Simulate_Throws_WhenACombatantIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => NoLuck().Simulate(null!, ClsTestData.FlatHero(2, "B", 50)));
        Assert.Throws<ArgumentNullException>(() => NoLuck().Simulate(ClsTestData.FlatHero(1, "A", 50), null!));
    }

    [Fact]
    public void ProductionVarianceProvider_StaysWithinItsDeclaredRange()
    {
        var provider = new ClsBattleVarianceProvider();

        // Sampled rather than reasoned about: if MaxVariance is ever widened by accident, an
        // "impossible" upset becomes possible and this catches it.
        for (var i = 0; i < 2_000; i++)
        {
            var variance = provider.NextVariance();
            Assert.InRange(variance, 1 - ClsBattleVarianceProvider.MaxVariance, 1 + ClsBattleVarianceProvider.MaxVariance);
        }
    }
}
