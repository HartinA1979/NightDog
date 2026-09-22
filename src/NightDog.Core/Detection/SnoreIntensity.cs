namespace NightDog.Core.Detection;

/// <summary>
/// Intensitätsstufe eines Schnarch-Fensters. Bestimmt die Farbe in allen Diagrammen
/// (siehe docs/Designsystem.md, Abschnitt „Schnarch-Intensität“).
/// </summary>
public enum SnoreIntensity
{
    None,
    Quiet,
    Medium,
    Loud,
}

public static class SnoreIntensityClassifier
{
    public const double DefaultDetectionThreshold = 0.3;
    public const double MediumThreshold = 0.5;
    public const double LoudThreshold = 0.75;

    /// <summary>
    /// Ordnet einen Schnarch-Score (0..1, Ausgabe des Klassifikators) einer Intensitätsstufe zu.
    /// </summary>
    public static SnoreIntensity FromScore(double score, double detectionThreshold = DefaultDetectionThreshold)
    {
        if (double.IsNaN(score))
            throw new ArgumentOutOfRangeException(nameof(score), "Score darf nicht NaN sein.");

        if (score < detectionThreshold)
            return SnoreIntensity.None;
        if (score < MediumThreshold)
            return SnoreIntensity.Quiet;
        if (score < LoudThreshold)
            return SnoreIntensity.Medium;
        return SnoreIntensity.Loud;
    }
}
