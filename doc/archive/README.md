# Archiv - Ursprüngliche Dokumentationsdateien

Dieses Verzeichnis enthält die ursprünglichen Einzeldokumente, die während der Entwicklung erstellt wurden. Sie wurden
in das **ENTWICKLERTAGEBUCH.md** im übergeordneten Verzeichnis zusammengefasst.

## Warum archiviert?

Die vielen Einzeldateien im Root-Verzeichnis des Projekts machten die Struktur unübersichtlich. Alle Informationen
wurden in einem strukturierten Entwicklertagebuch zusammengefasst, das:

- Einen chronologischen Überblick bietet
- Themen besser verknüpft
- Duplikate eliminiert
- Leichter zu navigieren ist

## Index der archivierten Dokumente

### Beleuchtung & Rendering

| Datei                          | Datum      | Status       | Beschreibung                                                                |
|--------------------------------|------------|--------------|-----------------------------------------------------------------------------|
| `BELEUCHTUNG_ERFOLGREICH.md`   | 27.01.2026 | ✅ 199 Zeilen | Erste erfolgreiche Beleuchtungs-Implementierung mit hart codierten Lichtern |
| `DYNAMISCHE_BELEUCHTUNG.md`    | 28.01.2026 | ✅ 324 Zeilen | Dynamische Lichter mit 3 Typen, std430-Layout                               |
| `LIGHTING_IMPLEMENTATION.md`   | 27.01.2026 | ✅ 269 Zeilen | Vollständige technische Dokumentation des Beleuchtungssystems               |
| `BACKFACE_CULLING_SCHATTEN.md` | 29.01.2026 | ✅ 181 Zeilen | Backface Culling für Rendering und Schatten                                 |

### Schatten-Implementierungen

| Datei                                | Datum      | Status       | Beschreibung                                                                  |
|--------------------------------------|------------|--------------|-------------------------------------------------------------------------------|
| `POISSON_DISK_SHADOWS_2026-01-29.md` | 29.01.2026 | ✅ 85 Zeilen  | Poisson Disk Sampling für gleichmäßige weiche Schatten                        |
| `PCSS_IMPLEMENTATION_2026-01-29.md`  | 29.01.2026 | ✅ 162 Zeilen | PCSS (Percentage-Closer Soft Shadows) - physikalisch korrekte weiche Schatten |
| `SIMPLE_HARD_SHADOWS_2026-01-29.md`  | 29.01.2026 | ✅ (gelesen)  | Rückkehr zu einfachen binären Schatten (PCSS entfernt)                        |

### Debugging & Fehlerbehebung

| Datei                             | Datum      | Status       | Beschreibung                                                  |
|-----------------------------------|------------|--------------|---------------------------------------------------------------|
| `DEBUG_SESSION.md`                | 27.01.2026 | ✅ 288 Zeilen | 3-stündige intensive Debug-Session zur Beleuchtung            |
| `FEHLERKORREKTUR_2026-01-28.md`   | 28.01.2026 | ✅ 75 Zeilen  | Korrekturen nach Rider-Absturz (Namespaces, Vector3 Operator) |
| `DREIECK_KORREKTUR_2026-01-28.md` | 28.01.2026 | ✅ 97 Zeilen  | Wiederherstellung des fehlenden mittleren Dreiecks            |

### Architektur & Struktur

| Datei                                   | Datum      | Status  | Beschreibung                                        |
|-----------------------------------------|------------|---------|-----------------------------------------------------|
| `ARCHITECTURE_CORRECTION_2026-01-28.md` | 28.01.2026 | ⚠️ Leer | Geplante Architektur-Korrekturen (nicht ausgefüllt) |
| `FLAT_ARCHITECTURE_2026-01-28.md`       | 28.01.2026 | ⚠️ Leer | Flache Namespace-Struktur (nicht ausgefüllt)        |
| `NAMING_CONVENTION_2026-01-28.md`       | 28.01.2026 | ⚠️ Leer | Namenskonventionen (nicht ausgefüllt)               |
| `REFACTORING_2026-01-28.md`             | 28.01.2026 | ⚠️ Leer | Refactoring-Dokumentation (nicht ausgefüllt)        |
| `USECASE_STRUCTURE_2026-01-28.md`       | 28.01.2026 | ⚠️ Leer | Use-Case Struktur (nicht ausgefüllt)                |

## Wichtige Erkenntnisse aus dem Archiv

### Debug-Session 27.01.2026 (3 Stunden)

**Problem**: Keine Beleuchtung sichtbar, nur flache Farben

**Root Cause**: Geometrie-Problem - alle Dreiecke hatten die gleiche Normale (waren parallel zueinander)

**Lösung**: Dreiecke mit unterschiedlichen Orientierungen:

- Rotes Dreieck: Normale in +Z Richtung
- Grünes Dreieck: Normale in +X Richtung
- Blaues Dreieck: Normale in -X Richtung

### std140 vs std430 Alignment-Problem

**Problem**: Light-Daten kamen nicht korrekt im Shader an

**Ursache**:

- C# `Vector3` = 12 bytes
- GLSL `vec3` in std140 = 16 bytes (aligned)

**Lösung**:

- Storage Buffer statt Uniform Buffer
- std430 Layout (einfacheres Alignment)
- Explizite float-Felder statt vec3/vec4

### Schatten-Evolution

**Phase 1**: Random Sampling → Clustering-Probleme  
**Phase 2**: Poisson Disk → Gleichmäßige Verteilung  
**Phase 3**: PCSS → Physikalisch korrekt, aber zu komplex  
**Phase 4**: Einfache harte Schatten → Schnell, ausreichend für Test-Szene

### Mouse Look Fix

**Problem**: Update-Reihenfolge falsch

**Lösung**: Consumer (CameraController) VOR Producer (InputHandler) updaten

## Verwendung der Archiv-Dateien

Diese Dateien dienen als:

1. **Historische Referenz** für Debugging-Sessions
2. **Detail-Quelle** für spezifische Implementierungen
3. **Backup** falls Informationen im Hauptdokument fehlen sollten

Bei Bedarf können spezifische Details aus diesen Dateien entnommen werden.

## Migration zum Entwicklertagebuch

Alle relevanten Informationen aus diesen Dateien wurden in das Entwicklertagebuch überführt und thematisch strukturiert:

- **Implementierungs-Chronologie** → Zeitliche Abfolge aller Features
- **Debugging-Sessions** → Zusammenfassung der wichtigsten Debug-Sessions
- **Technische Lösungen** → Konkrete Lösungen für spezifische Probleme
- **Gelernte Lektionen** → Erkenntnisse und Best Practices

---

**Archiviert**: 2026-01-29  
**Anzahl Dateien**: 15  
**Gesamtgröße**: ~2,000 Zeilen Dokumentation
