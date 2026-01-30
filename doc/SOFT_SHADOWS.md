# Soft Shadows Implementation

## Übersicht

Die Engine verwendet jetzt Soft Shadows mit Poisson-Disk Sampling für glatte Schattenübergänge ohne Kriselmuster (
Banding/Noise).

## Technik: Poisson-Disk Sampling mit rotierendem Pattern

### Problem mit einfachem Multi-Sampling

Einfaches Random-Sampling oder regelmäßige Grid-Patterns erzeugen sichtbare Muster:

- **Random Noise**: Kriselmuster, das bei Bewegung flackert
- **Regular Grid**: Banding-Artefakte an Schattenkanten

### Lösung: Poisson-Disk mit Per-Pixel Rotation

1. **Poisson-Disk Pattern**: 16 vorberechnete Sample-Positionen mit minimaler Clusterung
2. **Per-Pixel Rotation**: Jedes Pixel rotiert das Pattern basierend auf seiner Position
3. **Ergebnis**: Visuell kohärente, glatte Schatten ohne sichtbare Muster

## Implementation

### Shader-Code (pass2_lighting.comp)

```glsl
const vec2 POISSON_DISK[16] =  vec2[](
vec2(- 0.94201624, - 0.39906216),
vec2(0.94558609, - 0.76890725),
// ... 14 weitere Samples
);

float traceSoftShadow(vec3 origin, vec3 lightDir, float maxDist, float lightRadius, ivec2 pixelCoords) {
// Per-Pixel Rotation für Pattern-Variation
float rotation = float((pixelCoords.x * 73 + pixelCoords.y * 127) & 0xFF) * (PI / 128.0);

for (int i = 0; i < numSamples; i++) {
vec2 offset = POISSON_DISK[i] * softness;
// Rotiere Offset per Pixel
vec2 rotatedOffset = rotate(offset, rotation);
// Trace mit jittertem Richtungsvektor
vec3 jitteredDir = normalize(lightDir + tangent * rotatedOffset.x + bitangent * rotatedOffset.y);
// ...
}
}
```

## RenderSettings

| Preset           | ShadowSamples | ShadowSoftness | Qualität          |
|------------------|---------------|----------------|-------------------|
| UltraPerformance | 0             | 0.0            | Keine Shadows     |
| Performance      | 8             | 0.03           | Gute Qualität     |
| Default          | 12            | 0.04           | Hohe Qualität     |
| Quality          | 16            | 0.06           | Maximale Qualität |

## Parameter

### ShadowSamples (1-16)

- Anzahl der Shadow Rays pro Pixel
- Mehr Samples = glattere Schatten, aber langsamer
- 8 Samples sind ein guter Kompromiss

### ShadowSoftness (0.0-0.1)

- Stärke der Streuung um den Lichtvektor
- 0.0 = Harte Schatten
- 0.03-0.05 = Natürliche Soft Shadows
- > 0.1 = Sehr weiche, aber unrealistische Schatten

### Lichttyp-spezifisches Verhalten

#### Directional Light

- Simuliert entfernte Lichtquelle (z.B. Sonne)
- Fester `lightRadius = 1.0`
- Parallele Shadow Rays

#### Point Light

- Simuliert Punktlichtquelle (z.B. Lampe)
- `lightRadius = max(0.5, distance * 0.1)`
- Größerer Radius bei größerer Entfernung → realistischere Penumbra

## Performance

| Samples | Relative Performance |
|---------|----------------------|
| 1       | 100% (Baseline)      |
| 4       | ~75%                 |
| 8       | ~55%                 |
| 12      | ~40%                 |
| 16      | ~30%                 |

## Warum kein Temporal Accumulation?

Temporal Accumulation (Sammeln von Samples über mehrere Frames) würde:

- Glattere Schatten bei weniger Samples pro Frame ermöglichen
- Aber: Ghosting-Artefakte bei Bewegung erzeugen
- Zusätzlichen G-Buffer für Velocity erfordern

Aktuell: Per-Frame Soft Shadows mit Poisson-Disk sind ein guter Kompromiss.

## Zukünftige Verbesserungen

1. **Percentage Closer Soft Shadows (PCSS)**: Schattenweichheit abhängig von Blocker-Distanz
2. **Temporal Filtering**: Samples über Frames akkumulieren mit Motion Vectors
3. **Denoising Pass**: AI-basiertes Denoising für weniger Samples
