# Dynamische Beleuchtung

## 2026-01-29

## Status: ✅ Vollständig Implementiert und Funktionsfähig

Die dynamische Beleuchtung ist vollständig implementiert mit:

- 3 Lichttypen (Ambient, Directional, Point)
- Backface Culling für Rendering und Schatten
- Gamma-Korrektur für besseren Kontrast
- Konfigurierbare Attenuation für Point Lights

## Backface Culling

### Rendering

Nur die **Vorderseite** von Dreiecken wird gerendert. Die Rückseite ist unsichtbar.

```glsl
// In intersectTriangle()
if (det < EPSILON) return h; // Backface Culling
```

### Schatten

Nur die **Vorderseite** von Dreiecken wirft Schatten. Da der Shadow-Ray vom Punkt zum Licht geht (also aus der
entgegengesetzten Richtung), muss die Logik invertiert werden:

```glsl
// In intersectTriangleShadow()
if (det > - EPSILON) return h; // Invertiertes Backface Culling
```

**Warum invertiert?**

- Der Shadow-Ray geht vom Oberflächenpunkt **zum Licht**
- Er trifft Dreiecke von der **Rückseite** aus
- Mit `det > -EPSILON` blockieren nur Dreiecke, deren **Vorderseite** zum Licht zeigt

## Wichtige Korrektur: std430 statt std140

### Problem

Die Lichtdaten wurden nicht korrekt an den Shader übertragen.

### Lösung

1. **Storage Buffer statt Uniform Buffer** für Lichtdaten
2. **std430 Layout** statt std140 (einfacheres Alignment)
3. **Explizite float-Felder** statt vec3/vec4 in der Light-Struktur

## Lichtquellen-System

### Unterstützte Lichttypen

| Type | Name        | Beschreibung                    | Parameter                   |
|------|-------------|---------------------------------|-----------------------------|
| 0    | Ambient     | Gleichmäßige Grundbeleuchtung   | intensity, color            |
| 1    | Directional | Parallele Lichtstrahlen (Sonne) | direction, intensity, color |
| 2    | Point       | Punktförmige Lichtquelle        | position, intensity, color  |

### Aktuelle Szenen-Konfiguration

```csharp
scene.AddLight(LightEntity.CreateAmbient(Color.White, 0.02f));
scene.AddLight(LightEntity.CreateDirectional(new Vector3(0.5f, -1.0f, 0.5f), Color.White, 0.5f));
scene.AddLight(LightEntity.CreatePoint(new Vector3(-3, 4, 2), new Color(1.0f, 0.9f, 0.8f), 2.0f));
```

### Empfohlene Intensitätswerte

| Lichttyp    | Min | Standard | Max | Bemerkung                      |
|-------------|-----|----------|-----|--------------------------------|
| Ambient     | 0.0 | 0.02     | 0.1 | Höhere Werte → flache Schatten |
| Directional | 0.3 | 0.5      | 1.0 | Hauptlichtquelle               |
| Point       | 0.5 | 2.0      | 5.0 | Mit starker Attenuation        |

## Attenuation (Point Light)

Das Licht nimmt mit der Entfernung ab:

```glsl
float attenuation = 1.0 / (1.0 + 0.09 * dist + 0.032 * dist * dist);
```

## Gamma-Korrektur

Für besseren Kontrast wird Gamma-Korrektur angewendet:

```glsl
color = pow(color, vec3(1.0 / 2.2));
```

## Shader-Implementierung

### Light-Struktur (std430, 64 bytes)

```glsl
struct Light {
    int type;           // 0=Ambient, 1=Directional, 2=Point
    float intensity;
    float pad1, pad2;
    float posX, posY, posZ, pad3;
    float dirX, dirY, dirZ, pad4;
    float colorR, colorG, colorB, pad5;
};

layout (std430, binding 3) buffer LightUBO {
    int numLights;
    int _pad1, _pad2, _pad3;
    Light lights[8];
} lighting;
```

### Zwei Intersect-Funktionen

1. **intersectTriangle()** - für Rendering mit Backface Culling (`det < EPSILON`)
2. **intersectTriangleShadow()** - für Schatten mit invertiertem Backface Culling (`det > -EPSILON`)

### Zwei Trace-Funktionen

1. **trace()** - für primäre Rays und Reflektionen (verwendet `intersectTriangle`)
2. **traceShadowRay()** - für Shadow-Rays (verwendet `intersectTriangleShadow`)

## Winding Order

Die Vertex-Reihenfolge bestimmt die Normale (Counter-Clockwise = Vorderseite):

```
v0 → v1 → v2 (CCW von vorne gesehen)
Normal = cross(v1-v0, v2-v0)
```

### Beispiel: Boden (Normale nach oben)

```csharp
// Dreieck 1
new Vector3(-5, 0, -5),  // v0: hinten-links
new Vector3(-5, 0, 5),   // v1: vorne-links
new Vector3(5, 0, 5),    // v2: vorne-rechts

// Dreieck 2
new Vector3(-5, 0, -5),  // v0: hinten-links
new Vector3(5, 0, 5),    // v1: vorne-rechts
new Vector3(5, 0, -5),   // v2: hinten-rechts
```

## Features

### Beleuchtung

- ✅ Ambient Light (gleichmäßige Grundhelligkeit)
- ✅ Directional Light (paralleles Licht mit Richtung)
- ✅ Point Light (Punktlicht mit Attenuation)
- ✅ Bis zu 8 Lichter pro Szene
- ✅ Blinn-Phong Specular Highlights
- ✅ Gamma-Korrektur

### Schatten

- ✅ Hard Shadows (1 Sample)
- ✅ Soft Shadows (Monte Carlo, 1-16 Samples)
- ✅ Shadow Factor: 0.1 (Schatten) bis 1.0 (beleuchtet)
- ✅ Nur Vorderseiten werfen Schatten

### Backface Culling

- ✅ Rückseiten werden nicht gerendert
- ✅ Rückseiten werfen keine Schatten
- ✅ Konsistentes Verhalten

## Geänderte Dateien

| Datei                                                     | Änderung                                           |
|-----------------------------------------------------------|----------------------------------------------------|
| `Core/Scene/Light/LightEntity.cs`                         | LightType Enum (0=Ambient, 1=Directional, 2=Point) |
| `Infrastructure/Rendering/Vulkan/Shaders/raytracing.comp` | std430, Backface Culling, Shadow-Culling, Gamma    |
| `Infrastructure/Vulkan/InternalVulkanRenderer.cs`         | StorageBuffer, LightUniformData                    |
| `Application/Scene/SceneBuilderService.cs`                | Optimierte Licht- und Dreieck-Konfiguration        |
