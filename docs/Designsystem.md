# NightDog – Designsystem

Visueller Entwurf (klickbar, alle Screens + Styleguide): https://claude.ai/artifact/DPTyontUBDdjoV7f9ByNfZ

Die Farben, die Schrift und die Formen sind von [ha-vision.at](https://ha-vision.at) abgeleitet: Primärblau `#1769D2`, Türkis-Akzent `#14B8A6`, Nachtblau `#142033` als Text- bzw. Grundton, Inter, große Eckradien und Pill-Buttons. Die App startet im **dunklen Theme**, weil sie nachts im Schlafzimmer bedient wird. Ein helles Theme gibt es zusätzlich (Einstellungen → Darstellung: Dunkel / Hell / System).

## 1. Farben

Die Token-Namen entsprechen den späteren Schlüsseln in `Resources/Styles/Colors.xaml` (Phase 0). Im XAML wird jeweils `…Dark` / `…Light` definiert und per `AppThemeBinding` umgeschaltet.

### Markenfarben

| Token | Hex | Verwendung |
|---|---|---|
| `Primary` | `#1769D2` | Primär-Buttons, aktive Segmente, Schalter „an“ |
| `PrimaryDark` | `#0D4EA2` | Verlaufsende von Buttons, Pressed-State |
| `PrimaryBright` | `#4D94F0` | Primärfarbe als Text/Linie auf dunklem Grund (Kontrast), aktiver Tab |
| `Accent` | `#14B8A6` (Light: Text `#0F766E`) | Positive Werte, „ruhig/leise“, Kalibrierung |
| `AccentBright` | `#2DD4BF` | Akzent auf dunklem Grund |
| `Loud` | `#F5A524` (Light: Text `#B45309`) | Laute Schnarchphasen, Maximalwerte, Warnungen |
| `Danger` | `#F0616D` (Light: `#C8303D`) | Löschen, Nacht beenden |

### Neutrale Farben

| Token | Dark | Light |
|---|---|---|
| `Bg` | `#0A1220` | `#F5F8FC` |
| `Surface` | `#111C2E` | `#FFFFFF` |
| `Surface2` | `#172538` | `#EEF4FB` |
| `Line` | `#22324A` | `#D9E5F2` |
| `Text` | `#E9EFF8` | `#142033` |
| `TextSoft` | `#A3B3C9` | `#46566D` |
| `TextMuted` | `#7C8DA6` | `#66768B` |

Transparente Varianten für Chips und Hinweisboxen: `PrimarySoft` = Primärfarbe mit 10–14 % Deckkraft, `AccentSoft`/`LoudSoft` analog mit 12–16 %.

### Schnarch-Intensität (Diagramme)

Diese Farbskala gilt in allen Diagrammen (Timeline, Live-Pegel, Episodenliste) gleich:

| Stufe | Farbe | Bedeutung |
|---|---|---|
| Kein Schnarchen | `Line` | Grundpegel |
| Leise | `AccentBright` / `Accent` | Score < 0,5 |
| Mittel | `PrimaryBright` | Score 0,5–0,75 |
| Laut | `Loud` | Score > 0,75 |

Der Schnarch-Score 0–100 verwendet dieselbe Logik: < 30 Akzent, 30–59 Primär, ≥ 60 Laut.

## 2. Typografie

Schrift: **Inter** (als `.ttf` in `Resources/Fonts` einbinden, Schnitte 300/400/500/600/700/800). Zahlen immer mit Tabellenziffern, damit Zeiten und Werte nicht springen.

| Stil | Größe / Gewicht | Einsatz |
|---|---|---|
| Display | 76 / Light 300, Laufweite −4 % | Uhr im Live-Screen |
| H1 | 26–28 / ExtraBold 800, −2 % | Seitentitel |
| H2 | 17 / Bold 700 | Abschnittstitel |
| Kennzahl | 24 / ExtraBold 800 | Stat-Kacheln |
| Body | 15 / Regular 400, Zeilenhöhe 1,5 | Fließtext |
| Label | 15 / SemiBold 600 | Listeneinträge |
| Caption | 13 / Medium 500 | Untertitel, Metadaten |
| Eyebrow | 12 / Bold 700, Versalien, +12 % | Abschnittslabel in Einstellungen |

## 3. Formen & Abstände

- **Eckradien** (wie auf der Homepage): XS 10 · SM 16 · MD 22 · LG 30 · Pill 999
  - Karten: 22 · Kacheln und Listenelemente: 16–18 · Segment-Controls: 14 (innen 10) · Buttons, Chips, Schalter: Pill
- **Abstände:** 4er-Raster (4, 8, 12, 16, 20, 24, 32). Seitenrand 24, Kartenpadding 16–20, Abstand zwischen Karten 12.
- **Rahmen statt Schatten** im dunklen Theme: 1 px `Line`. Schatten nur auf dem Primär-Button (farbiges Glühen `rgba(23,105,210,0.35)`) und dem Start-Button.
- **Touch-Ziele** mindestens 44 × 44 px, primäre Buttons 56 px hoch.

## 4. Komponenten

| Komponente | Beschreibung | MAUI-Umsetzung (Phase 0) |
|---|---|---|
| Primär-Button | Pill, 56 px, Verlauf `#2A7BE4 → Primary`, weiße Schrift 16/700 | `Button` mit Style `PrimaryButton` + `LinearGradientBrush` |
| Sekundär-Button | Pill, `Surface` + 1 px `Line` | Style `SecondaryButton` |
| Start-Button | Kreis 188 px, radialer Verlauf, zwei weiche Ringe | `Border` mit `RoundRectangle`/`Ellipse` + `TapGestureRecognizer` |
| Karte | `Surface`, Radius 22, Rahmen `Line` | `Border` Style `Card` |
| Stat-Kachel | Label (12/600 muted), Wert (24/800), Untertitel | `ContentView` `StatTile` |
| Chip | Pill 36 px; aktiv = `PrimarySoft` + Rahmen `PrimaryBright` + Häkchen | `ContentView` oder `CollectionView`-Template |
| Schalter | 52 × 32, an = `Primary` | `Switch` mit `OnColor`/`ThumbColor` |
| Segment-Control | Hintergrund `Bg`, aktives Segment `Primary` | `ContentView` mit Grid + Bindings |
| Score-Ring | Kreisbogen 10 px, Farbe nach Score | SkiaSharp oder LiveCharts2 `PieChart` (Gauge) |
| Timeline | Balken pro 10 min, Farbe nach Intensität | LiveCharts2 `ColumnSeries` |
| Wochen-Balken + Ø-Linie | Balken, gestrichelte Durchschnittslinie in Akzent | LiveCharts2 `ColumnSeries` + `Section` |
| Tab-Leiste | 4 Tabs, aktiver Tab mit Pill-Hintergrund `PrimarySoft` | Shell `TabBar` + Custom Renderer/Handler für Pill |

Icons sind Linien-Icons mit 1,8 px Strichstärke und runden Enden. Vorschlag: [Lucide](https://lucide.dev) als Font oder SVG, ohne Emojis.

## 5. Screens

| # | Screen | Inhalt |
|---|---|---|
| 1 | Start | Start-Button, Schalter „Schnarchen aufnehmen“ (Entscheidung vor jeder Nacht), Start-Verzögerung, Auto-Stopp, Kurzinfo letzte Nacht |
| 2 | Kalibrierung | Countdown-Ring, gemessenes Grundrauschen, Warnhinweis bei lauter Umgebung |
| 3 | Live-Überwachung | Laufzeit, Live-Pegel, Status „Schnarchen erkannt“, Episoden/Clips, „Nacht beenden“ (gedrückt halten) |
| 4 | Morgen-Zusammenfassung | Score-Ring, 4 Kennzahlen, Nacht-Timeline, Faktoren-Abfrage |
| 5 | Nacht-Details | Große Timeline mit Legende, Episodenliste mit Wiedergabe und Favoriten |
| 6 | Verlauf & Trends | Woche/Monat/Jahr, Balkendiagramm mit Ø-Linie, Liste der Nächte |
| 7 | Faktoren-Tagebuch | Vergleich mit/ohne Faktor, Hinweis bei zu wenigen Nächten |
| 8 | Einstellungen | Aufnahme, Aufbewahrung, Speicher, Empfindlichkeit, Kalibrierung, Theme, Datenschutz, Löschen, Disclaimer |
| 9 | Onboarding | Berechtigungen erklären (Mikrofon, Benachrichtigung, Akku), Fortschrittspunkte |

## 6. Gestaltungsregeln für die Nacht

- Im Live-Screen keine großen hellen Flächen. Nach 30 s Inaktivität wird der Bildschirm abgedunkelt; die Uhr bleibt in Light 300 gut lesbar.
- Status wird immer mit Farbe **und** Text bzw. Icon angezeigt, nie mit Farbe allein.
- Texte erreichen einen Kontrast von mindestens 4,5:1. Deshalb wird `PrimaryBright` statt `Primary` für Text auf dunklem Grund verwendet.
