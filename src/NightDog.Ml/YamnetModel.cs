namespace NightDog.Ml;

/// <summary>
/// Eckdaten des YAMNet-Modells (siehe docs/Entwicklungsplan.md, Phase 3).
/// </summary>
public static class YamnetModel
{
    /// <summary>Erwartete Abtastrate der Eingabe in Hz (Mono, float im Bereich [-1, 1]).</summary>
    public const int SampleRate = 16_000;

    /// <summary>Länge eines Analysefensters in Samples (0,975 s).</summary>
    public const int WindowSamples = 15_600;

    /// <summary>Schrittweite zwischen zwei Fenstern in Samples (0,48 s, ca. 50 % Überlappung).</summary>
    public const int HopSamples = 7_680;

    /// <summary>Anzahl der AudioSet-Klassen in der Ausgabe.</summary>
    public const int ClassCount = 521;

    /// <summary>Dateiname des Modells in Resources/Raw der App.</summary>
    public const string ModelFileName = "yamnet.onnx";

    /// <summary>Dateiname der Klassenliste in Resources/Raw der App.</summary>
    public const string ClassMapFileName = "yamnet_class_map.csv";
}
