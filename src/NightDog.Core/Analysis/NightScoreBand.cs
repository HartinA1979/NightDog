namespace NightDog.Core.Analysis;

/// <summary>
/// Einstufung des Schnarch-Scores (0–100) einer Nacht, u. a. für die Farbe des Score-Rings.
/// </summary>
public enum NightScoreBand
{
    Low,
    Moderate,
    High,
}

public static class NightScore
{
    public const int Min = 0;
    public const int Max = 100;
    public const int ModerateFrom = 30;
    public const int HighFrom = 60;

    public static NightScoreBand GetBand(int score)
    {
        if (score is < Min or > Max)
            throw new ArgumentOutOfRangeException(nameof(score), score, $"Score muss zwischen {Min} und {Max} liegen.");

        return score switch
        {
            >= HighFrom => NightScoreBand.High,
            >= ModerateFrom => NightScoreBand.Moderate,
            _ => NightScoreBand.Low,
        };
    }
}
