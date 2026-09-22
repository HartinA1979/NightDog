# NightDog – Entwicklungsplan

Android-App (.NET MAUI), die nach dem Start über Nacht im Hintergrund mithört, Schnarchen erkennt, auswertet (Häufigkeit, Dauer, Lautstärke) und optional kurze Audio-Ausschnitte der Schnarchphasen aufnimmt.

## 1. Getroffene Entscheidungen

| Thema | Entscheidung |
|---|---|
| Plattform | Android 8.0+ (API 26), .NET 10 (LTS), .NET MAUI |
| Schnarch-Erkennung | ML-Modell **YAMNet** (AudioSet, 521 Klassen, u. a. „Snoring“), konvertiert nach ONNX |
| ML-Runtime | `Microsoft.ML.OnnxRuntime` (offizielles NuGet mit Android-Support) |
| Aufnahmen | Nur kurze Ausschnitte je Schnarchepisode (mit Vorlauf aus einem Ringpuffer). **Der User entscheidet vor jeder Nacht, ob aufgenommen wird** (Standardwert in den Einstellungen) |
| Speicherung | Ausschließlich lokal: SQLite (`sqlite-net-pcl`) + Audiodateien im App-internen Speicher |
| UI / MVVM | `CommunityToolkit.Mvvm`, `CommunityToolkit.Maui` |
| Diagramme | `LiveChartsCore.SkiaSharpView.Maui` (LiveCharts2) |
| Zusatzfunktionen | Verlauf & Trends, Faktoren-Tagebuch, Kalibrierung des Hintergrundgeräuschs, Start-Verzögerung und Auto-Stopp |

## 2. Architektur

### 2.1 Solution-Struktur

```
NightDog.sln
├── src/
│   ├── NightDog.App/            # .NET MAUI (net10.0-android)
│   │   ├── Views/               # XAML-Seiten
│   │   ├── ViewModels/          # CommunityToolkit.Mvvm
│   │   ├── Services/            # Plattformneutrale Service-Interfaces + Implementierungen
│   │   ├── Resources/Raw/       # yamnet.onnx, yamnet_class_map.csv
│   │   └── Platforms/Android/   # Foreground-Service, AudioRecord, MediaCodec, Permissions
│   ├── NightDog.Core/           # net10.0 Klassenbibliothek, KEINE MAUI-Abhängigkeit
│   │   ├── Audio/               # Ringpuffer, Pegelberechnung (RMS/dBFS), Resampling
│   │   ├── Detection/           # ISnoreClassifier, Episoden-Erkennung (Hysterese)
│   │   ├── Analysis/            # Statistik, Nacht-Score, Trends, Korrelationen
│   │   └── Data/                # Entities, Repositories (sqlite-net-pcl)
│   └── NightDog.Ml/             # ONNX-Wrapper für YAMNet (net10.0)
├── tests/
│   ├── NightDog.Core.Tests/     # xUnit, Tests mit WAV-Testdateien
│   └── NightDog.Ml.Tests/
└── tools/
    └── model-conversion/        # Python-Skript: YAMNet (TF Hub) → ONNX
```

Die ganze Erkennungs- und Auswertelogik liegt in `NightDog.Core`/`NightDog.Ml`. So lässt sie sich am PC mit aufgenommenen WAV-Dateien testen, ohne Emulator.

### 2.2 Laufzeit-Pipeline (während der Nacht)

```
AudioRecord (16 kHz, Mono, PCM16)
   │  Blöcke à 100 ms
   ▼
Ringpuffer (Vorlauf, z. B. 5 s) ──────────────────────────┐
   │                                                       │
   ▼                                                       │
Pegel (RMS → dBFS, + Kalibrier-Offset)                    │
   │                                                       │
   ▼                                                       │
Energie-Gate: Pegel < Grundrauschen + X dB? ── ja → „Stille“ (kein ML, spart Akku)
   │ nein                                                  │
   ▼                                                       │
YAMNet (ONNX), Fenster 0,975 s, Schrittweite ~0,5 s        │
   │  Score „Snoring“ (+ „Breathing“ zur Plausibilisierung)│
   ▼                                                       │
Episoden-Detektor (Hysterese, Glättung)                    │
   │  EpisodeStarted / EpisodeEnded                        │
   ├──► SQLite (Events, Minuten-Aggregate)                 │
   └──► Clip-Recorder (falls Aufnahme aktiv) ◄─────────────┘
          AAC/M4A via MediaCodec + MediaMuxer
```

## 3. Phasen

### Phase 0 – Projekt-Setup
1. Solution und Projekte wie in 2.1 anlegen, `TargetFramework` `net10.0-android`, `SupportedOSPlatformVersion` 26.
2. NuGet-Pakete: `CommunityToolkit.Mvvm`, `CommunityToolkit.Maui`, `sqlite-net-pcl`, `SQLitePCLRaw.bundle_green`, `Microsoft.ML.OnnxRuntime`, `LiveChartsCore.SkiaSharpView.Maui`, `xunit`.
3. Dependency Injection in `MauiProgram.cs` (Services als Singleton, ViewModels/Pages als Transient).
4. Navigation per Shell: *Start*, *Verlauf*, *Nacht-Detail*, *Tagebuch*, *Einstellungen*.
5. CI (GitHub Actions): Build + Unit-Tests für `Core`/`Ml`, Android-Build als Artefakt.

**Ergebnis:** Leere, lauffähige App mit Navigation; Tests laufen in CI.

### Phase 1 – Audio-Aufnahme im Hintergrund (Kernrisiko zuerst)
Das ist der technisch kritischste Teil, deshalb kommt er vor die Erkennung.

1. **Berechtigungen** (`AndroidManifest.xml`):
   - `RECORD_AUDIO` (Laufzeit-Abfrage über `Permissions.Microphone`)
   - `FOREGROUND_SERVICE`, `FOREGROUND_SERVICE_MICROPHONE` (Pflicht ab Android 14)
   - `POST_NOTIFICATIONS` (Laufzeit-Abfrage ab Android 13)
   - `WAKE_LOCK`
2. **Foreground-Service** `SleepMonitorService` (`Platforms/Android`):
   - `[Service(ForegroundServiceType = ForegroundService.TypeMicrophone)]`
   - Dauerhafte Benachrichtigung („NightDog hört zu – seit 23:14“) mit Aktion **Stopp**.
   - **Wichtig:** Ab Android 14 darf ein Mikrofon-Service nur gestartet werden, **während die App sichtbar ist**. Der Start erfolgt deshalb immer per Button in der App. Eine Start-Verzögerung wird *innerhalb* des bereits laufenden Service umgesetzt (siehe Phase 6), nicht über einen späteren Hintergrund-Start.
   - `PARTIAL_WAKE_LOCK` für die Dauer der Sitzung, damit die CPU beim ausgeschalteten Display weiterläuft.
3. **Audio-Erfassung** mit `Android.Media.AudioRecord`:
   - 16 kHz, Mono, PCM16, `AudioSource.VoiceRecognition` (weniger automatische Verstärkung/Rauschunterdrückung als `Mic`; auf Testgeräten vergleichen).
   - Eigener Lese-Thread, Blöcke à 100 ms → an `IAudioFrameSink` (Interface in `Core`) weiterreichen.
4. **Kommunikation App ↔ Service:** Ein Singleton `IMonitoringSession` (Status, Live-Pegel, Laufzeit) mit Events/`IObservable`, an das die UI bindet, solange die App offen ist.
5. **Robustheit:**
   - Hinweis-Dialog zur Akku-Optimierung (`ACTION_REQUEST_IGNORE_BATTERY_OPTIMIZATIONS` bzw. Einstellungsseite öffnen). Hersteller wie Samsung, Xiaomi und Huawei beenden Hintergrunddienste aggressiv; Referenz: dontkillmyapp.com.
   - Sitzungsstatus regelmäßig in der DB speichern, damit Daten nach einem Absturz oder Kill nicht verloren gehen. Eine abgebrochene Sitzung wird beim nächsten App-Start als „unvollständig“ markiert.
   - Umgang mit Audio-Fokus/Anrufen: Unterbrechung loggen, danach weiter aufnehmen.

**Ergebnis:** Die App kann 8+ Stunden bei ausgeschaltetem Display mithören und zeigt einen Live-Pegel an. Das wird mit einem echten Nachttest auf mindestens zwei Geräten geprüft (Akku-Verbrauch messen).

### Phase 2 – Pegelmessung & Kalibrierung
1. `LevelMeter` (Core): RMS pro Block → dBFS (`20·log10(rms)`), Glättung (gleitender Mittelwert), Spitzenwert.
2. **Lautstärke-Einheit:** Handy-Mikrofone liefern keinen echten Schalldruckpegel (dB SPL). Angezeigt werden deshalb **relative dB**, also der Abstand zum kalibrierten Grundrauschen. Das ist für den Vergleich zwischen Nächten aussagekräftiger als ein unkalibrierter Absolutwert. Optional gibt es in den Einstellungen einen manuellen Offset für eine geschätzte dB(A)-Anzeige.
3. **Kalibrierung** (zu Beginn jeder Sitzung, ca. 20–30 s, User soll leise sein):
   - Grundrauschen = Median-Pegel des Kalibrierzeitraums (bzw. 20. Perzentil).
   - Warnung, wenn das Grundrauschen sehr hoch ist (Ventilator, Klimaanlage) oder das Mikrofon abgedeckt scheint.
   - Das Grundrauschen dient als Referenz für das Energie-Gate und die relative Lautstärke.
   - Optional wird das Grundrauschen in der Nacht langsam nachgeführt (adaptiv), weil sich die Umgebung ändern kann (Heizung, Verkehr am Morgen).

**Ergebnis:** Kalibrier-Screen; Live-Pegelanzeige relativ zum Grundrauschen.

### Phase 3 – ML-Schnarcherkennung (YAMNet + ONNX)
1. **Modellkonvertierung** (`tools/model-conversion`, einmalig, Python):
   - YAMNet von TF Hub/Kaggle Models laden, mit `tf2onnx` nach ONNX konvertieren. Eingabe: Float-Waveform 16 kHz im Bereich [-1, 1], festes Fenster von 15 600 Samples (0,975 s) wie beim TFLite-Modell, damit die Tensor-Formen in C# einfach bleiben.
   - Ausgabe prüfen: Scores `[1, 521]` für die AudioSet-Klassen. Die Indizes von „Snoring“ (im offiziellen Class-Map Index 38) sowie „Breathing“ und „Speech“ aus `yamnet_class_map.csv` auslesen und **nicht hartcodieren**.
   - Ergebnis per Referenz-WAV mit dem Original-TF-Modell vergleichen (gleiche Scores ± Toleranz).
   - Optional: Quantisierung (INT8), um Größe (~15 MB → ~4 MB) und CPU-Last zu reduzieren; danach die Genauigkeit erneut prüfen.
2. **`YamnetClassifier : ISnoreClassifier`** (`NightDog.Ml`):
   - `InferenceSession` einmalig erstellen (Modell aus `Resources/Raw` via `FileSystem.OpenAppPackageFileAsync` laden), Session wiederverwenden.
   - Eingabe: PCM16 → float/32768. Fenster 0,975 s, Schrittweite ~0,5 s (50 % Überlappung).
   - Ausgabe: `SnoreScore`, `BreathingScore`, `SpeechScore` mit Zeitstempel.
   - Inferenz auf einem Hintergrund-Thread, nie auf dem Audio-Lese-Thread (entkoppelt über `System.Threading.Channels`).
3. **Energie-Gate:** Ist der Pegel nur knapp über dem Grundrauschen, wird keine Inferenz ausgeführt. Das spart einen großen Teil der Rechenzeit, weil die Nacht überwiegend still ist.
4. **Episoden-Detektor** (`Core`, reine Logik, gut testbar):
   - Glättung der Scores (z. B. Median über 3 Fenster).
   - **Start:** Snore-Score ≥ Schwelle (Startwert 0,3, konfigurierbar) in mindestens N von M Fenstern (z. B. 3 von 5).
   - **Ende:** keine Schnarch-Fenster für länger als T Sekunden (z. B. 15 s). Schnarchen ist rhythmisch, zwischen den Atemzügen liegen Pausen, die keine neue Episode auslösen sollen.
   - Mindestdauer einer Episode (z. B. 10 s), sonst verwerfen.
   - Pro Episode: Start, Ende, Dauer, Anzahl Schnarchlaute, Ø-/Max-Lautstärke (relativ), Ø-Score.
5. **Schwellwerte validieren:** Eigene Test-Aufnahmen (Schnarchen, Husten, Reden, Ventilator, Straßenlärm, Partner-Schnarchen aus größerer Entfernung) als WAV in `tests/` ablegen und Precision/Recall als Unit-Test absichern.

**Bekannte Einschränkung:** Ein Handy-Mikrofon kann nicht zuverlässig unterscheiden, **wer** schnarcht. Der Hinweis „Handy nah am Kopf platzieren“ kommt in die App. Eine Lautstärke-Plausibilisierung (leise Episoden = eher weiter entfernt) ist eine spätere Option.

**Ergebnis:** Schnarchepisoden werden live erkannt und protokolliert; Unit-Tests mit Referenzaufnahmen.

### Phase 4 – Datenhaltung
SQLite mit `sqlite-net-pcl`, Datei in `FileSystem.AppDataDirectory`.

| Tabelle | Wichtige Felder |
|---|---|
| `SleepSession` | Id, StartUtc, EndUtc, AnalysisStartUtc (nach Verzögerung), NoiseFloorDb, RecordingEnabled, Status (Running/Completed/Aborted), Notiz |
| `SnoreEpisode` | Id, SessionId, StartUtc, EndUtc, SnoreCount, AvgLevelDb, MaxLevelDb, AvgScore |
| `MinuteAggregate` | SessionId, MinuteIndex, AvgLevelDb, MaxLevelDb, SnoreSeconds (für die Nacht-Timeline, klein genug für ganze Nächte) |
| `AudioClip` | Id, EpisodeId, FilePath, DurationMs, SizeBytes, CreatedUtc, IsFavorite |
| `Factor` | Id, Name, Icon, IsBuiltIn (z. B. Alkohol, Erkältung, spätes Essen, Rückenlage, Sport, Medikamente) |
| `SessionFactor` | SessionId, FactorId |

- Repositories hinter Interfaces (`ISessionRepository` …) in `Core`.
- Schema-Version in einer `Meta`-Tabelle, einfache Migrationen per Code.
- Schreibzugriffe während der Nacht werden gepuffert: Minuten-Aggregate einmal pro Minute schreiben, nicht pro Frame.

### Phase 5 – Audio-Ausschnitte (optional pro Nacht)
1. **Einstellung + Abfrage vor dem Start:** Schalter „Schnarchgeräusche aufnehmen“ auf dem Start-Screen, vorbelegt mit dem Standardwert aus den Einstellungen. Ohne Aktivierung wird **kein** Audio gespeichert; der Ringpuffer lebt dann nur im RAM und wird laufend überschrieben.
2. **Clip-Recorder:**
   - Bei `EpisodeStarted`: Vorlauf aus dem Ringpuffer (5 s) + laufendes Audio an einen AAC-Encoder (`MediaCodec` + `MediaMuxer` → `.m4a`, 64 kbit/s mono, ca. 0,5 MB/min).
   - Maximale Clip-Länge 30 s. Bei langen Episoden wird nur der Anfang aufgenommen; optional ein weiterer Clip, wenn die Lautstärke deutlich über dem bisherigen Maximum liegt („lautester Moment“).
   - Limit pro Nacht (z. B. max. 20 Clips) und insgesamt (Speicherbudget, z. B. 200 MB).
3. **Aufräumen:** Automatische Löschung nach X Tagen (Einstellung: 7/14/30/nie), Favoriten sind ausgenommen. Manuelles Löschen einzelner Clips und „alle Aufnahmen löschen“.
4. **Wiedergabe** in der Nacht-Detailansicht mit `CommunityToolkit.Maui.MediaElement`.
5. Dateien liegen im App-internen Speicher (`AppDataDirectory/clips`): nicht für andere Apps sichtbar und bei Deinstallation mit entfernt. Für das Android-Auto-Backup werden Clips ausgeschlossen (`dataExtractionRules`/`fullBackupContent`).

### Phase 6 – Timer: Start-Verzögerung & Auto-Stopp
1. Auf dem Start-Screen: **Start-Verzögerung** (0/15/30/45/60 min, „Einschlafzeit“) und **Auto-Stopp** (Uhrzeit, z. B. 07:00, oder Dauer).
2. Die Umsetzung erfolgt komplett im laufenden Foreground-Service (wegen der Android-14-Regel aus Phase 1):
   - Während der Verzögerung läuft der Service, analysiert aber noch nicht (Mikrofon erst danach öffnen, spart Akku). Die Benachrichtigung zeigt „Überwachung startet um 23:45“.
   - Auto-Stopp über einen Timer im Service. Zusätzlich gibt es einen Fallback-Alarm via `AlarmManager.SetExactAndAllowWhileIdle` (erfordert ggf. `SCHEDULE_EXACT_ALARM` bzw. `USE_EXACT_ALARM`), falls der Timer im Doze-Modus verzögert wird.
3. Die Kalibrierung findet unmittelbar nach Ablauf der Verzögerung statt. Alternativ wird sie schon beim Drücken von Start durchgeführt und nach der Verzögerung adaptiv nachgeführt (siehe Phase 2).

### Phase 7 – Auswertung & UI
1. **Start-Screen:** Großer Start/Stopp-Button, Aufnahme-Schalter, Timer-Einstellungen, Hinweis zur Platzierung des Handys, Live-Status während der Sitzung.
2. **Morgen-Zusammenfassung** (nach Stopp):
   - Aufnahmedauer, Schnarchzeit gesamt und in % der Aufnahmedauer
   - Anzahl Episoden, längste Episode
   - Ø- und Max-Lautstärke (relativ)
   - **Schnarch-Score 0–100** (Kombination aus Anteil, Lautstärke und Häufigkeit; die Formel wird in `Core` gekapselt und getestet)
   - Abfrage der Tagebuch-Faktoren für diese Nacht
3. **Nacht-Detail:**
   - Timeline-Diagramm (LiveCharts2): Lautstärke pro Minute als Fläche, Schnarchphasen farbig hinterlegt
   - Liste der Episoden mit Uhrzeit, Dauer, Lautstärke und ▶-Button für den Clip
4. **Verlauf & Trends:**
   - Kalender-/Listenansicht aller Nächte mit Score
   - Diagramme Woche/Monat: Schnarchminuten pro Nacht, Score-Verlauf, gleitender 7-Tage-Durchschnitt
5. **Faktoren-Tagebuch:**
   - Faktoren pro Nacht an- und abwählen (vordefiniert + eigene)
   - Auswertung: Ø-Schnarchzeit *mit* vs. *ohne* Faktor (einfacher Mittelwertvergleich, Hinweis auf geringe Aussagekraft bei wenigen Nächten, z. B. erst ab 5 Nächten je Gruppe anzeigen)
6. **Einstellungen:** Standard Aufnahme an/aus, Aufbewahrungsdauer, Speicherbudget, Empfindlichkeit (Schwellwert), dB-Offset, Kalibrierdauer, alle Daten löschen.
7. **Onboarding:** Erklärung der Berechtigungen (Mikrofon, Benachrichtigungen, Akku-Optimierung), Datenschutzhinweis, Platzierungshinweis.

### Phase 8 – Test, Optimierung, Veröffentlichung
1. **Tests:**
   - Unit-Tests: Episoden-Detektor, Pegel, Score, Statistik, Faktor-Auswertung (Core)
   - ML-Regressionstests mit WAV-Referenzdateien
   - Mehrere echte Nachttests auf verschiedenen Geräten (mindestens ein Samsung- und ein Pixel-Gerät, möglichst ein Xiaomi), jeweils mit Akku-Verbrauch, Anzahl Fehlalarme und verpasster Episoden (Abgleich mit Clips)
2. **Performance-Ziele:** unter 10 % Akku pro Nacht (8 h), kein Speicherwachstum über die Nacht (Memory-Profiling mit `dotnet-counters`/Android Studio Profiler).
3. **Release-Build:** AOT/Trimming prüfen (ONNX Runtime und sqlite-net nutzen Reflection/Native-Libs, daher Trimming-Warnungen beachten), App-Größe kontrollieren (nur `arm64-v8a`, ggf. zusätzlich `armeabi-v7a`).
4. **Google Play:**
   - Deklaration des Foreground-Service-Typs *microphone* mit Begründung und Video im Play-Console-Formular
   - Formular „Datensicherheit“: Audio wird nur lokal verarbeitet, keine Weitergabe
   - Datenschutzerklärung (DSGVO), auch wenn keine Daten das Gerät verlassen
   - **Medizinischer Disclaimer:** Die App ist kein Medizinprodukt und stellt keine Diagnose (z. B. Schlafapnoe). Bei Verdacht auf Atemaussetzer ärztlich abklären lassen. Formulierungen im Store-Text so wählen, dass die App nicht unter die MDR fällt.

## 4. Reihenfolge & Meilensteine

| Meilenstein | Inhalt | Ziel |
|---|---|---|
| **M1 – Technischer Durchstich** | Phase 0, 1, 2 | Stabiles Mithören über Nacht, Pegel sichtbar |
| **M2 – Erkennung** | Phase 3, 4 | Schnarchepisoden zuverlässig erkannt und gespeichert |
| **M3 – MVP** | Phase 5, 6, Teile von 7 (Start, Zusammenfassung, Nacht-Detail) | Erste echte Nutzung durch dich selbst |
| **M4 – Vollversion** | Rest von Phase 7 (Verlauf, Trends, Tagebuch, Onboarding) | Feature-komplett |
| **M5 – Release** | Phase 8 | Play Store |

## 5. Risiken

| Risiko | Gegenmaßnahme |
|---|---|
| Hersteller beenden den Service in der Nacht | Foreground-Service + Wake Lock + Akku-Optimierungs-Ausnahme, Checkpoints in der DB, Tests auf Samsung/Xiaomi |
| Fehlalarme (Partner, Haustier, Ventilator) | ML statt Schwellwert, Kalibrierung, Hysterese, einstellbare Empfindlichkeit, Platzierungshinweis |
| YAMNet-Konvertierung nach ONNX macht Probleme | Früh in Phase 3 validieren. Fallback: Mel-Spektrogramm in C# berechnen und nur den Klassifikator-Teil als ONNX nutzen, oder TFLite-Bindings |
| Akku-Verbrauch zu hoch | Energie-Gate vor der Inferenz, quantisiertes Modell, größere Schrittweite |
| Unkalibrierte dB-Werte | Relative Lautstärke zum Grundrauschen statt Absolutwerte |
| Android-14/15-Einschränkungen für Hintergrund-Mikrofon | Start ausschließlich aus der sichtbaren App, Verzögerung im laufenden Service |
