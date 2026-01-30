# Smooth Shading Implementation

## Übersicht

Die Engine unterstützt jetzt drei Shading-Modi für Geometrie:

- **Flat Shading**: Jedes Dreieck hat eine einheitliche Normale (scharfe Kanten sichtbar)
- **Smooth Shading**: Vertex-Normalen werden interpoliert (glatte Oberfläche)
- **HalfSmooth**: Obere Hälfte glatt, untere Hälfte kantig (Demo-Effekt)

## Verwendung

### ShadingMode Enum

```csharp
public enum ShadingMode
{
    Flat,       // Face Normal für alle Vertices
    Smooth,     // Interpolierte Vertex-Normalen
    HalfSmooth  // Obere Hälfte glatt, untere Hälfte flach
}
```

### Beispiel: Kugel mit Smooth Shading

```csharp
GeometryGenerator.AddSphere(
    scene,
    center: new Vector3(0, 1, 0),
    radius: 0.8f,
    rings: 12,
    segments: 16,
    color: new Color(0.0f, 1.0f, 0.0f),
    shading: ShadingMode.Smooth  // Glatte Oberfläche
);
```

### Beispiel: Zylinder mit HalfSmooth

```csharp
GeometryGenerator.AddCylinder(
    scene,
    center: new Vector3(-2, 1, 0),
    radius: 0.5f,
    height: 2.0f,
    segments: 16,
    color: new Color(1.0f, 0.0f, 0.0f),
    shading: ShadingMode.HalfSmooth  // Demo-Effekt
);
```

## Technische Implementierung

### Vertex-Normalen in TriangleEntity

Jedes Dreieck speichert drei Vertex-Normalen:

```csharp
public class TriangleEntity
{
    public Vector3 V0, V1, V2;  // Vertices
    public Vector3 N0, N1, N2;  // Vertex-Normalen
    public Color Color;
}
```

- **Flat Shading**: N0 = N1 = N2 = FaceNormal
- **Smooth Shading**: Normale = Richtung vom Zentrum zum Vertex

### GPU-Datenstruktur

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct TriangleData
{
    public Vector3 V0, V1, V2;  // Vertices mit Padding
    public Vector3 Color;
    public Vector3 N0, N1, N2;  // Vertex-Normalen mit Padding
}
```

### Shader: Baryzentrische Interpolation

Im `pass1_primary.comp` Shader werden die Vertex-Normalen interpoliert:

```glsl
if (t > EPSILON) {
h.hit = true;
h.dist = t;
h.point = ray.origin + ray.direction * t;

// Baryzentrische Koordinaten: w + u + v = 1
float w = 1.0 - u - v;
vec3 interpolatedNormal = normalize(w * tri.n0 + u * tri.n1 + v * tri.n2);
h.normal = interpolatedNormal;
h.color = tri.color;
}
```

## Normalen-Berechnung

### Kugel

Für Kugeln zeigt die Normale radial nach außen:

```csharp
Vector3 n = (vertex - center).Normalized;
```

### Zylinder (Mantel)

Für den Zylinder-Mantel liegt die Normale in der XZ-Ebene:

```csharp
Vector3 n = new Vector3(x, 0, z).Normalized;
```

Die Deckel verwenden immer Flat Shading (Normale = (0, ±1, 0)).

## Visueller Effekt

| Shading Mode | Beschreibung                                             |
|--------------|----------------------------------------------------------|
| Flat         | Polygone deutlich sichtbar, facettierter Look            |
| Smooth       | Glatte, organische Oberfläche                            |
| HalfSmooth   | Obere Hälfte glatt, untere kantig (Demonstrationszwecke) |

## Performance

Die Vertex-Normalen erhöhen den Speicherbedarf pro Dreieck:

| Version              | Bytes pro Dreieck |
|----------------------|-------------------|
| Ohne Vertex-Normalen | 64 Bytes          |
| Mit Vertex-Normalen  | 112 Bytes         |

Die Berechnung im Shader bleibt gleich effizient, da nur eine Normalisierung hinzukommt.

## Erweiterte Szenarien

### Smooth Shading nur für bestimmte Objekte

Da ShadingMode pro AddXxx-Aufruf gesetzt wird, können verschiedene Objekte unterschiedliche Modi haben:

```csharp
// Glatte Kugel
GeometryGenerator.AddSphere(scene, pos1, 0.5f, 12, 16, Color.Red, ShadingMode.Smooth);

// Facettierter "Low-Poly" Würfel
GeometryGenerator.AddCube(scene, pos2, 1.0f, Color.Blue);  // Flat ist Default
```

### Zukünftige Erweiterungen

1. **Normal Maps**: Normalen aus Texturen
2. **Per-Triangle Shading Flag**: Einzelne Dreiecke mit unterschiedlichem Shading
3. **Edge Detection**: Automatische Erkennung von harten Kanten (z.B. bei Würfeln)
