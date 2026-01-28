# Fehlerkorrektur: Fehlendes mittleres Dreieck
## 2026-01-28

## Problem
Nach dem großen Refactoring wurde das mittlere Dreieck nicht mehr gerendert. Die Szene zeigte nur noch:
- Rotes Dreieck (links)
- Grünes Dreieck (rechts, aber falsche Position)
- Boden (2 Dreiecke)

**Fehlendes Dreieck**: Das mittlere (grüne) Dreieck mit Normale in +X Richtung und das blaue Dreieck mit Normale in -X Richtung waren verschwunden.

## Ursache
Beim Refactoring wurden die Dreieck-Definitionen in `Application/Scene/SceneBuilderService.cs` nicht korrekt übernommen. Statt der drei unterschiedlich orientierten Dreiecke aus der funktionierenden Version (dokumentiert in `BELEUCHTUNG_ERFOLGREICH.md`) waren nur noch zwei falsche Dreiecke vorhanden:

**Vorher (falsch):**
```csharp
// Rotes Dreieck - OK
scene.AddTriangle(new TriangleEntity(
    new Vector3(-2, 0, -1),
    new Vector3(-1, 2, -1),
    new Vector3(-1, 0, -1),
    new Color(1.0f, 0.0f, 0.0f)
));

// Grünes Dreieck - FALSCHE POSITION (parallel zum roten)
scene.AddTriangle(new TriangleEntity(
    new Vector3(1, 0, -1),
    new Vector3(2, 2, -1),
    new Vector3(2, 0, -1),
    new Color(0.0f, 1.0f, 0.0f)
));

// Blaues Dreieck - FEHLT KOMPLETT!
```

## Lösung
Die drei Dreiecke mit unterschiedlichen Orientierungen wurden gemäß der Dokumentation wiederhergestellt:

**Nachher (korrekt):**
```csharp
// Rotes Dreieck - zeigt nach VORNE (Normale in +Z Richtung)
scene.AddTriangle(new TriangleEntity(
    new Vector3(-2, 0, -1),
    new Vector3(-1, 2, -1),
    new Vector3(-1, 0, -1),
    new Color(1.0f, 0.0f, 0.0f)
));

// Grünes Dreieck - zeigt nach RECHTS (Normale in +X Richtung)
scene.AddTriangle(new TriangleEntity(
    new Vector3(0, 0, -0.5f),
    new Vector3(0, 2, 0),
    new Vector3(0, 0, 0.5f),
    new Color(0.0f, 1.0f, 0.0f)
));

// Blaues Dreieck - zeigt nach LINKS (Normale in -X Richtung)
scene.AddTriangle(new TriangleEntity(
    new Vector3(2, 0, 0.5f),
    new Vector3(2, 2, 0),
    new Vector3(2, 0, -0.5f),
    new Color(0.0f, 0.0f, 1.0f)
));
```

## Beleuchtungs-Effekt
Die unterschiedlichen Orientierungen sind essentiell für die Beleuchtung:

- **Rotes Dreieck**: Normale zeigt nach vorne → 62% Helligkeit
- **Grünes Dreieck**: Normale zeigt nach rechts → 100% Helligkeit (optimal zum Licht)
- **Blaues Dreieck**: Normale zeigt nach links → 50% Helligkeit

Ohne unterschiedliche Orientierungen hätten alle Dreiecke die gleiche Helligkeit!

## Betroffene Dateien
- `Application/Scene/SceneBuilderService.cs` - Zeilen 22-48

## Verifikation
```bash
dotnet build
dotnet run
```

**Erwartetes Ergebnis:**
- 3 Dreiecke mit unterschiedlichen Farben und Helligkeiten
- Grünes Dreieck in der Mitte
- Blaues Dreieck rechts
- Rotes Dreieck links

## Dokumentations-Referenz
Die korrekten Dreieck-Positionen sind dokumentiert in:
- `BELEUCHTUNG_ERFOLGREICH.md` (Zeilen 38-62)
- `DEBUG_SESSION.md` (Zeilen 47-69)

## Wichtige Erkenntnis
Nach einem Refactoring müssen die funktionierenden Geometrie-Definitionen 1:1 aus der Dokumentation übernommen werden. Die Dreieck-Orientierungen sind nicht willkürlich, sondern gezielt gewählt für unterschiedliche Beleuchtungs-Effekte!
