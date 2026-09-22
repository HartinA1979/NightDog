using NightDog.Core.Analysis;

namespace NightDog.Core.Tests.Analysis;

public class NightScoreTests
{
    [Theory]
    [InlineData(0, NightScoreBand.Low)]
    [InlineData(29, NightScoreBand.Low)]
    [InlineData(30, NightScoreBand.Moderate)]
    [InlineData(59, NightScoreBand.Moderate)]
    [InlineData(60, NightScoreBand.High)]
    [InlineData(100, NightScoreBand.High)]
    public void GetBand_UsesDesignThresholds(int score, NightScoreBand expected)
    {
        Assert.Equal(expected, NightScore.GetBand(score));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void GetBand_RejectsOutOfRange(int score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NightScore.GetBand(score));
    }
}
