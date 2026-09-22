# NightDog
Überwachung ob geschnarcht wird – Android-App mit .NET MAUI (.NET 10).

- [Entwicklungsplan](docs/Entwicklungsplan.md)
- [Designsystem](docs/Designsystem.md)

## Projektstruktur

| Projekt | Zweck |
|---|---|
| `src/NightDog.App` | .NET MAUI App (nur `net10.0-android`, Android 8.0+) |
| `src/NightDog.Core` | Plattformunabhängige Logik: Audio, Erkennung, Auswertung, Daten |
| `src/NightDog.Ml` | YAMNet-Klassifikator über ONNX Runtime |
| `tests/NightDog.Core.Tests` | xUnit-Tests für `NightDog.Core` |

NuGet-Versionen werden zentral in `Directory.Packages.props` gepflegt.

## Bauen & Starten

Voraussetzungen: .NET 10 SDK, MAUI-Android-Workload (`dotnet workload install maui-android`), Android SDK (API 36) und JDK 17+ – am einfachsten über Visual Studio 2026 mit der Workload „.NET Multi-Platform App UI“.

```bash
# Tests
dotnet test tests/NightDog.Core.Tests

# App auf angeschlossenem Gerät / Emulator starten
dotnet build src/NightDog.App -t:Run -f net10.0-android
```

Die CI (GitHub Actions) baut `Core`/`Ml`, führt die Tests aus und erstellt ein APK als Build-Artefakt.

## Lizenzen

Schrift [Inter](https://github.com/rsms/inter) unter SIL Open Font License 1.1, siehe `src/NightDog.App/Resources/Licenses/Inter-OFL.txt`.
