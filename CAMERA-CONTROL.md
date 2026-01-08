# ✅ Maus-gesteuerte Kamera-Neigung Implementiert!

## Änderungen

### Problem behoben:
- ❌ Alte Kamera drehte sich im Kreis und die Welt ging aus dem Bild
- ✅ Kamera ist jetzt fest positioniert und neigt sich nur zur Mausposition

### Implementierte Features:

#### 1. **MouseHandler** (`Infrastructure/Input/MouseHandler.ts`)
- Trackt Maus-Position auf dem Canvas
- Berechnet normalisierte Koordinaten (-1 bis +1)
- Bietet `MouseState` Interface mit:
  - `x`, `y` - Absolute Pixel-Koordinaten
  - `normalizedX`, `normalizedY` - Normalisierte Koordinaten

#### 2. **CameraController** (`Application/Camera/CameraController.ts`)
- Steuert Kamera basierend auf Maus-Position
- **Konfigurierbar:**
  - `basePosition` - Feste Kamera-Position (Standard: 0, 5, 12)
  - `baseTarget` - Zentrum der Szene (Standard: 0, 0, 0)
  - `maxTiltAngle` - Maximale Neigung in Grad (Standard: 30°)

- **Funktionsweise:**
  - Maus in der Mitte → Kamera schaut geradeaus auf Zentrum
  - Maus nach oben → Kamera neigt sich nach oben (max. 30°)
  - Maus nach unten → Kamera neigt sich nach unten (max. 30°)
  - Maus nach links → Kamera neigt sich nach links (max. 30°)
  - Maus nach rechts → Kamera neigt sich nach rechts (max. 30°)

#### 3. **Engine Integration**
- `Engine.setUpdateCallback()` - Callback für Updates pro Frame
- Wird automatisch im Render-Loop aufgerufen
- Camera-Controller wird in jedem Frame aktualisiert

#### 4. **EngineController Erweiterung**
- Initialisiert `CameraController` automatisch
- Verbindet Camera-Updates mit Engine-Loop
- Dispose-Pattern für sauberes Cleanup

### Kamera-Konfiguration:

```typescript
// Standard-Werte
Position: (0, 5, 12) - 12 Einheiten vor und 5 Einheiten über der Szene
Target: (0, 0, 0) - Schaut auf Zentrum
Max Tilt: 30° - Moderate Neigung

// Anpassbar über CameraController:
cameraController.setBasePosition(new Vector3(x, y, z));
cameraController.setBaseTarget(new Vector3(x, y, z));
cameraController.setMaxTiltAngle(degrees);
```

### Technische Details:

**Berechnung der Kamera-Neigung:**
```typescript
tiltX = normalizedY * maxTiltAngle // Vertikale Maus → Vertikale Neigung
tiltY = normalizedX * maxTiltAngle // Horizontale Maus → Horizontale Neigung

adjustedTarget = baseTarget + offset basierend auf tilt
camera.lookAt(adjustedTarget)
```

**Update-Zyklus:**
```
Engine Render Loop
  → update(deltaTime)
    → updateCallback()
      → cameraController.update()
        → mouseHandler.getMouseState()
        → Berechne neue Target-Position
        → camera.lookAt(adjustedTarget)
```

### Verwendung:

```bash
# Dev-Server starten
npm run dev
```

**Interaktion:**
- Bewegen Sie die Maus über das Canvas
- Kamera folgt sanft der Maus-Position
- Maximale Neigung: 30° in alle Richtungen
- Kamera bleibt an fester Position (0, 2, 12)

### Vorteile:

✅ **Stabile Sicht** - Kamera bleibt an fester Position
✅ **Intuitive Steuerung** - Maus = Blickrichtung
✅ **Begrenzte Neigung** - Verhindert Desorientierung
✅ **Sanfte Bewegung** - Direkte Maus-Position-Mapping
✅ **Clean Architecture** - Separation of Concerns eingehalten

### Anpassungen:

Um die Neigung zu ändern, passen Sie den Wert beim Initialisieren an:

```typescript
// In CameraController.ts, Konstruktor
new CameraController(
    canvas,
    camera,
    new Vector3(0, 5, 12), // Position
    45 // Neigung in Grad (statt 30)
)
```

## Status: ✅ Vollständig Implementiert

Die Kamera-Steuerung ist nun maus-basiert mit maximal 30° Neigung!

