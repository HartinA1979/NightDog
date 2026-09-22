using NightDog.Core.Detection;

namespace NightDog.Core.Tests.Detection;

public class SnoreIntensityClassifierTests
{
    [Theory]
    [InlineData(0.0, SnoreIntensity.None)]
    [InlineData(0.29, SnoreIntensity.None)]
    [InlineData(0.3, SnoreIntensity.Quiet)]
    [InlineData(0.49, SnoreIntensity.Quiet)]
    [InlineData(0.5, SnoreIntensity.Medium)]
    [InlineData(0.74, SnoreIntensity.Medium)]
    [InlineData(0.75, SnoreIntensity.Loud)]
    [InlineData(1.0, SnoreIntensity.Loud)]
    public void FromScore_UsesDesignThresholds(double score, SnoreIntensity expected)
    {
        Assert.Equal(expected, SnoreIntensityClassifier.FromScore(score));
    }

    [Fact]
    public void FromScore_RespectsCustomDetectionThreshold()
    {
        Assert.Equal(SnoreIntensity.None, SnoreIntensityClassifier.FromScore(0.35, detectionThreshold: 0.4));
        Assert.Equal(SnoreIntensity.Quiet, SnoreIntensityClassifier.FromScore(0.45, detectionThreshold: 0.4));
    }

    [Fact]
    public void FromScore_RejectsNaN()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SnoreIntensityClassifier.FromScore(double.NaN));
    }
}
