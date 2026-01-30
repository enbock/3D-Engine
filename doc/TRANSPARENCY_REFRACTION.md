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
vec3 calculateCaustics(vec3 hitPoint, vec3 normal) {
    // Photon-Raytracing: Prüfe ob Licht durch transparentes Objekt kommt
    if (photonHit.hit && photonHit.transparency > 0.5) {
        float focusFactor = 1.0 + photonHit.ior * 0.2;
        causticColor += lightColor * intensity * focusFactor;
    }
    return causticColor;
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

~~1. **Dispersion**: Wellenlängenabhängige Brechung (Regenbogeneffekt)~~ ✅ Implementiert
~~2. **Volumetrische Kaustiken**: Lichtmuster im Volumen~~ ✅ Implementiert
~~3. **Photon Mapping**: Akkurate Kaustik-Berechnung~~ ✅ Implementiert
~~4. **Schlieren**: Optische Verzerrungen durch Temperaturdifferenzen~~ ✅ Implementiert

---

## Erweiterte Features (Neu)

### 5. Dispersion (Chromatische Aberration)

Wellenlängenabhängige Brechung erzeugt Regenbogeneffekte bei hohem IOR (z.B. Diamant).

**Cauchy-Formel:**

```glsl
float cauchyIOR(float baseIOR, float wavelength) {
    return baseIOR + CAUCHY_B / (wavelength * wavelength);
}
```

**Funktionsweise:**

- 3 separate Strahlen für Rot (0.65μm), Grün (0.55μm), Blau (0.45μm)
- Jede Wellenlänge hat leicht unterschiedlichen Brechungsindex
- Aktiviert automatisch bei IOR > 1.8 (Diamant, Kristall)

**Konstanten:**

```glsl
const float DISPERSION_STRENGTH = 0.02;
const float CAUCHY_B = 0.004;
const int DISPERSION_SAMPLES = 3;
```

### 6. Volumetrische Kaustiken

Lichtmuster innerhalb des Volumens zwischen Kamera und Oberfläche.

**Funktionsweise:**

- Ray-Marching mit 8 Samples entlang des Sichtstrahls
- Prüfung auf transparente Objekte zwischen Sample und Lichtquelle
- Physikalisch basierte Streuung mit Phase-Funktion
- Exponentieller Abfall über Distanz

**Formel:**

```glsl
float scattering = 0.02;
float attenuation = exp(-t * 0.1);
float phase = 0.25 / 3.14159;  // Isotrope Streuung
```

### 7. Photon Mapping (Erweiterte Kaustiken)

Akkuratere Kaustik-Berechnung durch Photon-Sampling.

**Funktionsweise:**

- 8 Photonen-Samples pro Pixel in einem Gather-Radius
- Gaussische Gewichtung basierend auf Entfernung
- Konvergenz-Faktor basierend auf Objekt-Distanz
- IOR-basierte Fokussierung

**Parameter:**

```glsl
const int PHOTON_SAMPLES = 8;
const float PHOTON_GATHER_RADIUS = 0.5;
```

**Algorithmus:**

1. Sample-Punkte im Kreis um Hit-Point generieren
2. Für jeden Sample: Strahl zur Lichtquelle senden
3. Prüfen ob transparentes Objekt im Weg
4. Photon-Power berechnen mit Gaussischer Gewichtung
5. Akkumulieren für finales Kaustik-Ergebnis

### 8. Schlieren-Effekt

Optische Verzerrungen durch simulierte Temperatur-/Dichtegradienten.

**Per-Material aktivierbar** - Der Schlieren-Effekt kann für jedes Material individuell ein- oder ausgeschaltet werden:

```csharp
// Glaskugel MIT Schlieren-Effekt
GeometryGenerator.AddGlassSphere(scene, position, radius, rings, segments, tint, ior: 1.52f, enableSchlieren: true);

// Glaskugel OHNE Schlieren-Effekt (Standard)
GeometryGenerator.AddGlassSphere(scene, position, radius, rings, segments, tint, ior: 1.52f, enableSchlieren: false);

// Oder direkt über MaterialEntity
MaterialEntity glass = MaterialEntity.Glass(color, ior: 1.52f, schlieren: true);
```

**3D Perlin Noise:**

```glsl
float noise3D(vec3 p) {
    // Smooth interpolated 3D noise
    vec3 i = floor(p);
    vec3 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);  // Smoothstep
    // ...
}
```

**Normal-Perturbation:**

```glsl
vec3 calculateSchlieren(vec3 hitPoint, vec3 normal, vec3 rayDir, float time) {
    vec3 noiseCoord = hitPoint * SCHLIEREN_FREQUENCY + vec3(0.0, time * 0.5, 0.0);

    float noiseX = noise3D(noiseCoord) * 2.0 - 1.0;
    float noiseY = noise3D(noiseCoord + vec3(100.0, 0.0, 0.0)) * 2.0 - 1.0;
    float noiseZ = noise3D(noiseCoord + vec3(0.0, 100.0, 0.0)) * 2.0 - 1.0;

    return normalize(normal + vec3(noiseX, noiseY, noiseZ) * SCHLIEREN_STRENGTH);
}
```

**Parameter:**

```glsl
const float SCHLIEREN_STRENGTH = 0.15;
const float SCHLIEREN_FREQUENCY = 3.0;
```

**Effekte:**

- Zeitabhängige Animation (via `camera.time`)
- Natürliche Verzerrungen wie bei heißer Luft
- Subtile Wellenbewegungen

---

## Performance-Übersicht (Optimiert)

### Vorher vs. Nachher

| Feature                 | Vorher              | Nachher            | Einsparung |
|-------------------------|---------------------|--------------------|------------|
| Dispersion              | 6 Rays (3×2)        | 2 Rays + Farbshift | ~66%       |
| Photon Mapping          | 8 Samples/Pixel     | 1 Ray/Lichtquelle  | ~87%       |
| Volumetrische Kaustiken | 8 Ray-March Steps   | 3 Steps + LOD      | ~62%       |
| Schlieren               | 3 Noise + Full Calc | Reduzierte Stärke  | ~30%       |

### Aktuelle Kosten pro Pixel

| Feature                | Rays/Samples | Bedingung                |
|------------------------|--------------|--------------------------|
| Standard Refraktion    | +2           | Immer bei Transparenz    |
| Dispersion (optimiert) | +2           | Nur bei IOR > 1.8        |
| Photon Caustics        | +1/Licht     | Nur opake Oberflächen    |
| Volumetric Caustics    | +3           | Nur bei Distanz < 8      |
| Schlieren              | +3 Noise     | Nur transparente Objekte |

### Optimierungs-Strategien

1. **Early-Exit bei niedrigem Beitrag**
   ```glsl
   if (fresnel > MIN_CONTRIBUTION) { ... }
   if (transmissionFactor > MIN_CONTRIBUTION) { ... }
   ```

2. **Distanz-basiertes LOD**
   ```glsl
   if (distToHit < 8.0) {
       volumetric = calculateVolumetricCausticsFast(...);
   }
   ```

3. **Vereinfachte Dispersion**
    - Nur 1 Raycast statt 3
    - Farbshift mathematisch berechnet statt physikalisch simuliert

4. **Kombinierte Caustics**
    - Photon + Simple Caustics zu einer Funktion zusammengefasst
    - Keine Multi-Sample-Gathering mehr

### Konstanten (angepasst)

```glsl
const float MIN_CONTRIBUTION = 0.01;      // Skip-Schwelle
const float SCHLIEREN_STRENGTH = 0.08;    // Reduziert von 0.15
const float SCHLIEREN_FREQUENCY = 2.0;    // Reduziert von 3.0
const float PHOTON_GATHER_RADIUS = 0.3;   // Reduziert von 0.5
```

**Empfehlungen:**

- Dispersion nur bei IOR > 1.8 aktiviert (automatisch)
- Volumetrische Kaustiken nur bei Nähe zur Kamera
- Schlieren dezent für natürlichen Look
