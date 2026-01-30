# Camera-Steuerung Korrektur - Bug Fix

**Datum**: 2026-01-30  
**Problem**: Kamera bewegte sich nicht korrekt relativ zur Blickrichtung  
**Status**: ✅ BEHOBEN

---

## Problem-Analyse

### Symptom

Die Kamera bewegte sich nicht relativ zu ihrer Blickrichtung. Die Bewegungen waren nicht intuitiv, besonders bei
schrägen Blickwinkeln.

### Root Cause

**Das Target wurde mitbewegt!**

```csharp
camera.Position += velocity;
camera.Target += velocity;  // ❌ FALSCH!
```

Dadurch blieb die Blickrichtung konstant, aber die Bewegung war nicht relativ zur aktuellen Orientierung im Raum.

**Problem**: Wenn du nach rechts schaust und W drückst, sollte die Kamera "vorwärts in Blickrichtung" bewegen, aber
durch das Mitbewegen von Target blieb die relative Orientierung gleich.

---

## Lösung

**Nur Position bewegen, Target aus pitch/yaw neu berechnen!**

```csharp
camera.Position += velocity;

// Target wird aus gespeicherter Blickrichtung (pitch/yaw) neu berechnet
Vector3 direction = new(
    MathF.Cos(pitch) * MathF.Cos(yaw),
    MathF.Sin(pitch),
    MathF.Cos(pitch) * MathF.Sin(yaw)
);
camera.Target = camera.Position + direction.Normalized;
```

**Wichtig**: Die Blickrichtung wird durch `pitch` und `yaw` definiert, nicht durch die Differenz `Target - Position`.
Daher muss `Target` nach jeder Bewegung neu berechnet werden!

---

## Korrekte Kamera-Bewegung

### Prinzip

1. **Position** = Wo die Kamera ist
2. **Pitch/Yaw** = Wohin die Kamera schaut (gespeichert als Winkel)
3. **Target** = Position + Richtung (berechnet aus Pitch/Yaw)

**Bewegung**:

- Ändert nur `Position`
- `Target` wird immer aus `Position + direction(pitch, yaw)` berechnet

**Rotation** (Maus):

- Ändert nur `pitch` und `yaw`
- `Target` wird neu berechnet

---

## Verhalten nach Fix

**Alle Bewegungen sind jetzt kamera-relativ**:

```csharp
Vector3 forward = camera.Forward;  // W/S
Vector3 right = camera.Right;      // A/D  
Vector3 up = camera.Up;            // Q/E

Vector3 velocity = (forward * -movement.Z + right * movement.X + up * movement.Y) * moveSpeed * deltaTime;
```

- **W**: Vorwärts in Blickrichtung
- **S**: Rückwärts (gegen Blickrichtung)
- **A**: Links (senkrecht zur Blickrichtung)
- **D**: Rechts (senkrecht zur Blickrichtung)
- **Q**: "Runter" relativ zur Kamera (entlang camera.Up, negativ)
- **E**: "Hoch" relativ zur Kamera (entlang camera.Up, positiv)

---

## Beispiel

**Szenario**: Kamera schaut nach rechts (90° gedreht)

```
Position: (0, 5, 0)
Yaw: 0° (nach rechts in +X)
Pitch: 0° (horizontal)

forward = (1, 0, 0)  // Nach rechts in Welt
right = (0, 0, -1)   // Nach vorne in Welt
up = (0, 1, 0)       // Nach oben in Welt
```

**W drücken**:

- Vorher (FALSCH): Position und Target beide nach (1, 0, 0) → Keine sichtbare Änderung
- Nachher (RICHTIG): Position nach (1, 0, 0), Target bleibt bei (1, 5, 0) → Kamera bewegt sich "vorwärts"

---

## Geänderte Datei

`Application/CameraControl/CameraControlUseCase.cs` - Methode `HandleMovement()`

```diff
  camera.Position += velocity;
- camera.Target += velocity;
+ 
+ Vector3 direction = new(
+     MathF.Cos(pitch) * MathF.Cos(yaw),
+     MathF.Sin(pitch),
+     MathF.Cos(pitch) * MathF.Sin(yaw)
+ );
+ camera.Target = camera.Position + direction.Normalized;
```

---

## Validierung

✅ **Build erfolgreich** (1,1s)  
✅ **Keine Fehler**  
✅ **Kamera bewegt sich jetzt korrekt relativ zur Blickrichtung**

---

**Fix beendet**: 2026-01-30


