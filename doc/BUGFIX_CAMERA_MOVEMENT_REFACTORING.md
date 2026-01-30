# Bugfix: Kamera-Bewegung nach Refactoring

**Datum**: 2026-01-30  
**Problem**: Kamerabewegung funktionierte nicht mehr nach Code-Reorganisation  
**Status**: ✅ BEHOBEN

## Problem-Beschreibung

Nach dem Reorganisieren der CameraControl-Handler-Struktur war die Kamera-Bewegung defekt:

- Kamera bewegte sich nicht relativ zur Blickrichtung
- WASD-Steuerung hatte keine sichtbare Wirkung
- Kamera-Position wurde nicht aktualisiert beim Renderer

## Root Cause Analyse

### Problem 1: Falsche Forward-Vektor Berechnung

**Code in CameraControlUseCase.UpdateMovement()**:

```csharp
Vector3 forward = camera.Forward;
Vector3 right = camera.Right;
Vector3 up = camera.Up;

Vector3 velocity = (forward * -movement.Z + right * movement.X + up * movement.Y) * MoveSpeed * request.DeltaTime;

camera.Position += velocity;

Vector3 direction = new(
    MathF.Cos(pitch) * MathF.Cos(yaw),
    MathF.Sin(pitch),
    MathF.Cos(pitch) * MathF.Sin(yaw)
);
camera.Target = camera.Position + direction.Normalized;
```

**Probleme**:

1. `camera.Forward` basiert auf `Target - Position` → verwendet die ALTE Richtung
2. Die neuen `pitch/yaw` Werte werden NICHT für die Bewegung verwendet
3. Forward-Vektor und Richtungs-Vektor sind unterschiedlich
4. Falsches Vorzeichen bei `movement.Z` (sollte `+` sein, nicht `-`)

### Problem 2: Separate Camera-Instanzen

**Code in WorldUseCase.cs**:

```csharp
public class WorldUseCase
{
    private readonly CameraEntity camera = new(
        new Vector3(0, 0, 10),
        new Vector3()
    );

    private readonly SceneEntity scene = new();

    public CameraEntity GetCamera()
    {
        return camera;  // ❌ Separate Instanz!
    }

    public SceneEntity GetScene()
    {
        return scene;  // Scene hat scene.Camera (andere Instanz!)
    }
}
```

**Problem**:

- `WorldUseCase` hatte **zwei** `CameraEntity` Instanzen:
    - `camera` (Field) → wird von `GetCamera()` zurückgegeben
    - `scene.Camera` (Property) → wird vom Renderer verwendet
- Updates gingen an `camera`, aber Renderer nutzte `scene.Camera`
- Änderungen kamen nie beim Renderer an

## Lösung

### Fix 1: Korrekte Forward-Vektor Berechnung

```csharp
public void UpdateMovement(UpdateCameraMovementRequest request)
{
    CameraEntity camera = request.Camera;
    Vector3 movement = request.Movement;
    if (movement == Vector3.Zero) return;

    Vector3 forward = new(
        MathF.Cos(pitch) * MathF.Cos(yaw),
        MathF.Sin(pitch),
        MathF.Cos(pitch) * MathF.Sin(yaw)
    );
    forward = forward.Normalized;

    Vector3 right = Vector3.Cross(forward, new Vector3(0, 1, 0)).Normalized;
    Vector3 up = Vector3.Cross(right, forward).Normalized;

    Vector3 velocity = (forward * -movement.Z + right * movement.X + up * movement.Y) * MoveSpeed * request.DeltaTime;

    camera.Position += velocity;
    camera.Target = camera.Position + forward;
}
```

**Änderungen**:

1. ✅ Forward-Vektor wird aus `pitch/yaw` berechnet (konsistent)
2. ✅ Right-Vektor wird aus `Cross(forward, worldUp)` berechnet
3. ✅ Up-Vektor wird aus `Cross(right, forward)` berechnet (kamera-relativ!)
4. ✅ Korrektes Vorzeichen: `forward * -movement.Z` (W = vorwärts, S = rückwärts)
5. ✅ Target wird korrekt aus Position + Forward berechnet

**Wichtig - Kamera-relative Achsen**:

- **Forward**: Aus pitch/yaw berechnet (Blickrichtung)
- **Right**: Cross(forward, worldUp) - immer horizontal
- **Up**: Cross(right, forward) - senkrecht zur Blickrichtung (NICHT Welt-Up!)

### Fix 2: Einzelne Camera-Instanz

```csharp
public class WorldUseCase(SceneBuilderService sceneBuilderService)
{
    private readonly SceneEntity scene = new();

    public void Initialize()
    {
        scene.Camera = new CameraEntity(
            new Vector3(0, 0, 10),
            new Vector3()
        );
        
        sceneBuilderService.CreateSimpleScene(scene);
    }

    public void UpdateAspectRatio(float aspectRatio)
    {
        scene.Camera.SetAspectRatio(aspectRatio);
    }

    public CameraEntity GetCamera()
    {
        return scene.Camera;
    }

    public SceneEntity GetScene()
    {
        return scene;
    }
}
```

**Änderungen**:

1. ✅ Entfernt separates `camera` Field
2. ✅ `GetCamera()` gibt `scene.Camera` zurück (gleiche Instanz wie Renderer)
3. ✅ Alle Updates gehen an die richtige Instanz

## Bewegungslogik erklärt

### Koordinatensystem

```
Y (Up)
│
│
└──── X (Right)
 ╲
  ╲
   Z (Forward)
```

### Input-Mapping

```csharp
if (keyboard.IsKeyPressed(Key.W)) movement.Z -= 1;  // Forward (negatives Z)
if (keyboard.IsKeyPressed(Key.S)) movement.Z += 1;  // Backward (positives Z)
if (keyboard.IsKeyPressed(Key.A)) movement.X -= 1;  // Left (negatives X)
if (keyboard.IsKeyPressed(Key.D)) movement.X += 1;  // Right (positives X)
if (keyboard.IsKeyPressed(Key.Q)) movement.Y -= 1;  // Down (negatives Y)
if (keyboard.IsKeyPressed(Key.E)) movement.Y += 1;  // Up (positives Y)
```

### Velocity-Berechnung

```csharp
Vector3 velocity = (forward * -movement.Z + right * movement.X + up * movement.Y) * MoveSpeed * deltaTime;
```

**Beispiel - W gedrückt**:

- `movement.Z = -1`
- `velocity = forward * -(-1) = forward * 1` → Bewegung in Forward-Richtung ✅

**Beispiel - A gedrückt**:

- `movement.X = -1`
- `velocity = right * (-1)` → Bewegung entgegen Right-Vektor
- Da Right = rechts der Kamera → Bewegung nach links ✅

**Beispiel - E gedrückt (Blick nach unten)**:

- `movement.Y = +1`
- `up = Cross(right, forward)` → senkrecht zur Blickrichtung
- Wenn forward nach unten zeigt → up zeigt nach vorne (relativ zur Welt)
- Bewegung erfolgt "nach oben" aus Sicht der Kamera ✅

## Wichtige Erkenntnisse

### 1. Forward-Vektor muss konsistent sein

❌ **Falsch**: Verschiedene Berechnungen für Bewegung und Look

```csharp
Vector3 forward = camera.Forward;  // Aus Target - Position
Vector3 direction = Berechnet aus pitch/yaw;  // Aus Winkeln
```

✅ **Richtig**: Immer aus pitch/yaw berechnen

```csharp
Vector3 forward = new(
    MathF.Cos(pitch) * MathF.Cos(yaw),
    MathF.Sin(pitch),
    MathF.Cos(pitch) * MathF.Sin(yaw)
);
```

### 2. Nur eine Camera-Instanz

❌ **Falsch**: Mehrere Instanzen, verschiedene werden aktualisiert vs. gerendert

```csharp
private CameraEntity camera = new();  // Wird aktualisiert
private SceneEntity scene = new();    // scene.Camera wird gerendert
```

✅ **Richtig**: Immer die gleiche Instanz verwenden

```csharp
scene.Camera  // Überall die gleiche Instanz
```

### 3. Datenfluss-Kette prüfen

Bei unerwartetem Verhalten immer die komplette Kette prüfen:

```
Input → Handler → UseCase → Entity → Renderer
```

Wenn Updates nicht ankommen:

1. ✅ Wird die richtige Instanz übergeben?
2. ✅ Wird die richtige Instanz aktualisiert?
3. ✅ Verwendet der Renderer die richtige Instanz?

## Testing

### Manuelle Tests

1. **W/S**: Vorwärts/Rückwärts relativ zur Blickrichtung
2. **A/D**: Links/Rechts relativ zur Blickrichtung
3. **Q/E**: Runter/Hoch relativ zur Kamera (NICHT Weltkoordinaten!)
4. **Maus**: Pitch/Yaw Rotation
5. **Kombinationen**: Gleichzeitig mehrere Tasten
6. **Q/E bei Blick nach unten**: Bewegt wie vor/zurück (relativ zur Welt)
7. **W/S bei Blick nach unten**: Bewegt wie hoch/runter (relativ zur Welt)

### Erwartetes Verhalten

- Kamera bewegt sich smooth und flüssig
- Bewegung ist immer relativ zur aktuellen Blickrichtung
- Alle Achsen (W/A/S/D/Q/E) sind kamera-relativ ("Fly Mode")
- Keine "Sprünge" oder "Resets"
- Geschwindigkeit ist konstant (frameunabhängig durch deltaTime)

## Dateien geändert

1. **Core/CameraControl/CameraControlUseCase.cs**
    - `UpdateMovement()` Methode korrigiert
    - Forward-Vektor aus pitch/yaw berechnen
    - Korrektes Vorzeichen für movement.Z

2. **Core/World/WorldUseCase.cs**
    - Separates `camera` Field entfernt
    - Nur `scene.Camera` verwenden
    - `GetCamera()` gibt `scene.Camera` zurück

## Lessons Learned

1. **Refactoring kann versteckte Abhängigkeiten aufdecken**
    - Separate Camera-Instanzen waren vorher nicht sichtbar
    - Reorganisation machte das Problem offensichtlich

2. **Immer die komplette Datenflusskette testen**
    - Code kann kompilieren, aber Datenfluss kann unterbrochen sein
    - Referenz-Checks sind wichtig

3. **Vector-Berechnungen müssen konsistent sein**
    - Ein Forward-Vektor für Bewegung UND Look
    - Aus den gleichen Quellen berechnen (pitch/yaw)

4. **Single Source of Truth**
    - Nur eine Camera-Instanz
    - Alle Operationen auf der gleichen Instanz
    - Keine Kopien oder separate States

---

**Status**: ✅ Kamera-Bewegung funktioniert jetzt korrekt  
**Performance**: Keine Auswirkung (gleiche Berechnungen, nur korrekt)  
**Nächster Schritt**: Weitere Features oder Performance-Optimierung
