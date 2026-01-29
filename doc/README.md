# Dokumentation - Vulkan 3D-Engine

## Übersicht

Dieses Verzeichnis enthält die gesamte technische Dokumentation des Projekts.

## Hauptdokument

- **[ENTWICKLERTAGEBUCH.md](./ENTWICKLERTAGEBUCH.md)** - Vollständiges Entwicklertagebuch mit:
    - Projektübersicht & Architektur
    - Chronologische Implementierungs-Historie
    - Debugging-Sessions mit Lösungen
    - Technische Lösungen & Best Practices
    - Gelernte Lektionen
    - Aktueller Status & Roadmap

## Archivierte Dokumente

Die ursprünglichen Einzeldokumente wurden in `archive/` verschoben und sind im Entwicklertagebuch zusammengefasst.

### Thematische Übersicht der archivierten Dokumente

#### Beleuchtung & Rendering

- `BELEUCHTUNG_ERFOLGREICH.md` - Initiale Beleuchtungs-Implementierung (hart codiert)
- `DYNAMISCHE_BELEUCHTUNG.md` - Dynamische Lichter mit 3 Typen
- `LIGHTING_IMPLEMENTATION.md` - Vollständige Beleuchtungs-Dokumentation
- `BACKFACE_CULLING_SCHATTEN.md` - Backface Culling & Schatten-Optimierung

#### Schatten-Techniken

- `POISSON_DISK_SHADOWS_2026-01-29.md` - Poisson Disk Sampling für weiche Schatten
- `PCSS_IMPLEMENTATION_2026-01-29.md` - PCSS (Percentage-Closer Soft Shadows)
- `SIMPLE_HARD_SHADOWS_2026-01-29.md` - Zurück zu einfachen harten Schatten

#### Debugging & Fehlerbehebung

- `DEBUG_SESSION.md` - 3-stündige Debug-Session zur Beleuchtung
- `FEHLERKORREKTUR_2026-01-28.md` - Korrekturen nach Rider-Absturz
- `DREIECK_KORREKTUR_2026-01-28.md` - Fehlendes mittleres Dreieck

#### Architektur & Refactoring

- `ARCHITECTURE_CORRECTION_2026-01-28.md` - Architektur-Korrekturen (leer)
- `FLAT_ARCHITECTURE_2026-01-28.md` - Flache Namespace-Struktur (leer)
- `NAMING_CONVENTION_2026-01-28.md` - Namenskonventionen (leer)
- `REFACTORING_2026-01-28.md` - Refactoring-Dokumentation (leer)
- `USECASE_STRUCTURE_2026-01-28.md` - Use-Case Struktur (leer)

## Verwendung

### Für neue Entwickler

Starte mit **ENTWICKLERTAGEBUCH.md** - es enthält alles Wichtige chronologisch aufbereitet.

### Für spezifische Themen

Nutze das Inhaltsverzeichnis im Entwicklertagebuch, um direkt zum relevanten Abschnitt zu springen.

### Für historische Details

Die archivierten Dokumente enthalten teilweise mehr Details zu einzelnen Debugging-Sessions.

## Struktur

```
doc/
├── README.md                    # Diese Datei
├── ENTWICKLERTAGEBUCH.md        # Hauptdokumentation
└── archive/                     # Archivierte Einzeldokumente
    ├── BELEUCHTUNG_ERFOLGREICH.md
    ├── DYNAMISCHE_BELEUCHTUNG.md
    ├── LIGHTING_IMPLEMENTATION.md
    ├── BACKFACE_CULLING_SCHATTEN.md
    ├── POISSON_DISK_SHADOWS_2026-01-29.md
    ├── PCSS_IMPLEMENTATION_2026-01-29.md
    ├── SIMPLE_HARD_SHADOWS_2026-01-29.md
    ├── DEBUG_SESSION.md
    ├── FEHLERKORREKTUR_2026-01-28.md
    ├── DREIECK_KORREKTUR_2026-01-28.md
    ├── ARCHITECTURE_CORRECTION_2026-01-28.md
    ├── FLAT_ARCHITECTURE_2026-01-28.md
    ├── NAMING_CONVENTION_2026-01-28.md
    ├── REFACTORING_2026-01-28.md
    └── USECASE_STRUCTURE_2026-01-28.md
```

## Wartung

Bei neuen Features oder Bugfixes:

1. Update **ENTWICKLERTAGEBUCH.md** im entsprechenden Abschnitt
2. Chronologische Einträge unter "Implementierungs-Chronologie" hinzufügen
3. Gelernte Lektionen unter "Gelernte Lektionen" dokumentieren
4. "Aktueller Status" aktualisieren

## Prinzipien

Diese Dokumentation folgt den gleichen Prinzipien wie der Code:

- **Clean**: Klar strukturiert, leicht zu navigieren
- **Vollständig**: Alle Entscheidungen dokumentiert
- **Ehrlich**: Fehler und Lösungen gleichermaßen dokumentiert
- **Aktuell**: Status immer up-to-date

---

**Letzte Aktualisierung**: 2026-01-29
