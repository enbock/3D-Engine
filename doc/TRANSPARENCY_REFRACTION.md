# Transparenz, Lichtbrechung und Photon-Effekte

## Übersicht

Die Engine unterstützt jetzt **transparente Materialien** mit physikalisch basierter Lichtbrechung (Refraktion),
Fresnel-Reflexion und Kaustik-Effekten.

## Features

### 1. MaterialEntity

Neues Material-System für Oberflächeneigenschaften:

```csharp
public class MaterialEntity
{
    public Color Color { get; set; }
    public float Transparency { get; set; }      // 0.0 = opak, 1.0 = vollständig transparent
    public float IndexOfRefraction { get; set; } // IOR (z.B. Glas = 1.52)
    public float Reflectivity { get; set; }      // Basis-Reflexivität
}
```

### 2. Vordefinierte Materialien

| Material | Transparency | IOR  | Reflectivity | Beschreibung              |
|----------|--------------|------|--------------|---------------------------|
| Opaque   | 0.0          | 1.0  | 0.0          | Standard undurchsichtig   |
| Glass    | 0.95         | 1.52 | 0.1          | Typisches Fensterglas     |
| Water    | 0.9          | 1.33 | 0.05         | Wasser                    |
| Diamond  | 0.98         | 2.42 | 0.15         | Diamant (hohe Dispersion) |

### 3. Physikalische Grundlagen

#### Snell'sches Gesetz (Brechungsgesetz)

```
n₁ · sin(θ₁) = n₂ · sin(θ₂)
```

Die Richtung des gebrochenen Strahls wird berechnet mit:

```glsl
vec3 refractedDir = refract(rayDir, normal, eta);
// eta = n₁/n₂ (Verhältnis der Brechungsindizes)
```

#### Fresnel-Reflexion (Schlick-Approximation)

```glsl
float fresnelSchlick(float cosTheta, float ior) {
    float r0 = (1.0 - ior) / (1.0 + ior);
    r0 = r0 * r0;
    return r0 + (1.0 - r0) * pow(1.0 - cosTheta, 5.0);
}
```

Der Fresnel-Effekt bewirkt:

- Bei flachem Blickwinkel: Mehr Reflexion
- Bei senkrechtem Blickwinkel: Mehr Transmission

#### Absorption (Beer-Lambert-Gesetz)

```glsl
vec3 absorption = exp(-tint * pathLength * 0.5);
```

Licht wird auf seinem Weg durch das Material entsprechend der Tint-Farbe absorbiert.

### 4. Kaustik-Effekte (Caustics)

Kaustiken entstehen durch Lichtkonzentration bei Brechung. Die Engine simuliert vereinfachte Kaustiken:

```glsl
vec3 calculateCaustics(vec3 hitPoint, vec3 normal, ivec2 pixelCoords) {
    // Photon-Raytracing: Prüfe ob Licht durch transparentes Objekt kommt
    if (photonHit.hit && photonHit.transparency > 0.5) {
        float focusFactor = 1.0 + photonHit.ior * 0.2;
        causticColor += lightColor * intensity * focusFactor;
    }
}
```

## Verwendung

### Glaskugel erstellen

```csharp
// Einfache Glaskugel (IOR = 1.52)
GeometryGenerator.AddGlassSphere(
    scene,
    center: new Vector3(-1, 0.6f, 2),
    radius: 0.6f,
    rings: 16,
    segments: 24,
    tint: new Color(0.95f, 0.98f, 1.0f),  // Leicht bläulich
    ior: 1.52f
);

// Diamant-Kugel (IOR = 2.42)
GeometryGenerator.AddDiamondSphere(
    scene,
    center: new Vector3(1, 0.6f, 2),
    radius: 0.5f,
    rings: 20,
    segments: 32,
    tint: new Color(1.0f, 1.0f, 1.0f)
);

// Wasser-Kugel (IOR = 1.33)
GeometryGenerator.AddWaterSphere(
    scene,
    center: new Vector3(0, 0.6f, 2),
    radius: 0.4f,
    rings: 12,
    segments: 20,
    tint: new Color(0.8f, 0.9f, 1.0f)
);
```

### Benutzerdefiniertes Material

```csharp
// Eigenes transparentes Material
MaterialEntity customGlass = new MaterialEntity(
    color: new Color(0.9f, 0.95f, 0.9f),  // Grünstich
    transparency: 0.92f,
    ior: 1.45f,                            // Niedriger als Standard-Glas
    reflectivity: 0.08f
);

scene.AddTriangle(new TriangleEntity(v0, v1, v2, customGlass, n0, n1, n2));
```

## Technische Implementierung

### G-Buffer Erweiterung

Material-Properties werden in den Alpha-Kanälen des G-Buffers gespeichert:

| Buffer    | RGB           | Alpha        |
|-----------|---------------|--------------|
| gPosition | Position      | 1.0 = Hit    |
| gNormal   | Normal        | Transparency |
| gAlbedo   | Albedo Color  | IOR          |
| gRayDir   | Ray Direction | Reflectivity |

### Pass 3: Reflections & Refraction

Der pass3_reflections.comp Shader wurde erweitert:

1. **Fresnel-Berechnung**: Bestimmt Reflexion/Refraktion-Verhältnis
2. **Refraktion-Raytracing**: Verfolgt gebrochene Strahlen
3. **Doppel-Brechung**: Eintritt und Austritt aus dem Objekt
4. **Absorption**: Farbliche Tönung basierend auf Pfadlänge
5. **Kaustiken**: Lichtkonzentration auf Oberflächen

### Referenz-Brechungsindizes

| Material | IOR       |
|----------|-----------|
| Vakuum   | 1.00      |
| Luft     | 1.0003    |
| Wasser   | 1.33      |
| Glas     | 1.45-1.65 |
| Kristall | 2.00      |
| Diamant  | 2.42      |

## Performance

| Feature    | Zusätzliche Rays pro Pixel |
|------------|----------------------------|
| Refraktion | +2 (Eintritt + Austritt)   |
| Kaustiken  | +1 pro Lichtquelle         |

Empfohlene Einstellungen für Performance:

- Niedrigere Tesselation für Glaskugeln (rings=12, segments=16)
- Kaustiken können bei Performance-Problemen deaktiviert werden

## Demo-Szene

Die Standard-Demo-Szene enthält nun eine Glaskugel:

- **Position**: (-1, 0.6, 2)
- **Radius**: 0.6
- **Material**: Glas (IOR = 1.52)
- **Tint**: Leicht bläulich

## Zukünftige Erweiterungen

1. **Dispersion**: Wellenlängenabhängige Brechung (Regenbogeneffekt)
2. **Volumetrische Kaustiken**: Lichtmuster im Volumen
3. **Photon Mapping**: Akkurate Kaustik-Berechnung
4. **Schlieren**: Optische Verzerrungen durch Temperaturdifferenzen
