# Indirect Lighting / Global Illumination

## Übersicht

Die Engine unterstützt jetzt indirekte Beleuchtung (Global Illumination) durch einen zusätzlichen Rendering-Pass.
Dieser Pass berechnet Licht, das von anderen Oberflächen reflektiert wird (One-Bounce Indirect Lighting).

## Architektur

### Erweitertes Pass-System

```
Pass 1:   Primary Rays    → G-Buffer (Position, Normal, Albedo, RayDir)
Pass 2:   Lighting        → Lit Color (Direct Diffuse + Specular + Shadows)
Pass 2B:  Indirect Light  → Indirect Color (One-Bounce GI)
Pass 3:   Reflections     → Reflected Color (Multi-Bounce Reflections)
Pass 4:   Composite       → Final Output (Gamma Correction + BGR Conversion)
```

### Neuer G-Buffer

| Image         | Format  | Inhalt                  | Verwendung       |
|---------------|---------|-------------------------|------------------|
| indirectColor | RGBA32F | Direct + Indirect Light | Input für Pass 3 |

## Technik: One-Bounce Path Tracing

### Cosine-Weighted Hemisphere Sampling

Für jeden Pixel wird eine zufällige Richtung in der Hemisphäre der Oberflächen-Normale generiert:

```glsl
vec3 randomCosineDirection(vec3 normal, vec2 seed) {
    float u1 = hash(seed);
    float u2 = hash(seed + vec2(17.0, 31.0));

    float r = sqrt(u1);
    float theta = 2.0 * PI * u2;

    float x = r * cos(theta);
    float y = r * sin(theta);
    float z = sqrt(max(0.0, 1.0 - u1));

    // Transform to world space using tangent frame
    vec3 tangent = normalize(cross(up, normal));
    vec3 bitangent = cross(normal, tangent);

    return normalize(tangent * x + bitangent * y + normal * z);
}
```

### Poisson-Disk Rotation

Um sichtbare Muster zu vermeiden, wird für jeden Pixel eine individuelle Rotation des Sampling-Patterns verwendet:

```glsl
float rotation = float((pixelCoords.x * 73 + pixelCoords.y * 127) & 0xFF) * (PI / 128.0);
```

### One-Bounce Berechnung

1. Ray von Hit-Point in zufällige Hemisphären-Richtung schießen
2. Bei Treffer: Direct Light an diesem Punkt berechnen
3. Farbe mit Albedo des ursprünglichen Punkts multiplizieren
4. Falloff basierend auf Distanz anwenden

## RenderSettings

### Neue Parameter

| Parameter  | Typ   | Beschreibung                      | Bereich    |
|------------|-------|-----------------------------------|------------|
| EnableGi   | bool  | GI ein/ausschalten                | true/false |
| GiSamples  | int   | Anzahl der Bounce-Rays pro Pixel  | 1-16       |
| GiStrength | float | Stärke der indirekten Beleuchtung | 0.0-1.0    |

### Presets

| Preset           | EnableGi | GiSamples | GiStrength | Performance Impact |
|------------------|----------|-----------|------------|--------------------|
| UltraPerformance | false    | 0         | 0.0        | Kein               |
| Performance      | true     | 2         | 0.3        | Gering (~1-2ms)    |
| Default          | true     | 4         | 0.5        | Mittel (~2-4ms)    |
| Quality          | true     | 4         | 0.6        | Mittel (~2-4ms)    |

## Shader: pass2b_indirect.comp

### Bindings

```
Binding 0: gPosition      (StorageImage, Read)
Binding 1: gNormal        (StorageImage, Read)
Binding 2: gAlbedo        (StorageImage, Read)
Binding 3: litColor       (StorageImage, Read)
Binding 4: indirectColor  (StorageImage, Write)
Binding 5: TriangleSSBO   (StorageBuffer)
Binding 6: LightUBO       (StorageBuffer)
Binding 7: RenderSettings (UniformBuffer)
Binding 8: CameraUBO      (UniformBuffer)
Binding 9: BvhSSBO        (StorageBuffer)
```

### Algorithmus

```glsl
vec3 calculateIndirectLight(hitPoint, normal, albedo, pixelCoords) {
    for (int i = 0; i < giSamples; i++) {
        // Generiere zufällige Richtung mit Rotation
        vec3 bounceDir = randomCosineDirection(normal, seed);

        // Trace Ray
        Hit bounceHit = traceBVH(bounceRay);

        if (bounceHit.hit) {
            // Berechne direktes Licht am Bounce-Punkt
            vec3 bounceLight = calculateDirectLight(bounceHit);

            // Akkumuliere mit Falloff
            indirectSum += bounceLight * albedo * falloff;
        } else {
            // Sky Light beitragen
            indirectSum += getSkyColor(bounceDir) * albedo;
        }
    }

    return (indirectSum / giSamples) * giStrength;
}
```

## Performance

### Optimierungen (v2)

Die GI-Implementierung wurde für bessere Performance optimiert:

1. **Keine Shadow-Traces für Bounce-Light**: Der größte Performance-Gewinn. Für indirekte Beleuchtung werden keine
   Shadow-Rays gecastet.

2. **Kürzere Max-Distanz**: `GI_MAX_DIST = 15.0` statt 100.0. GI-Rays müssen nicht weit reisen.

3. **Kleinerer BVH-Stack**: Stack von 32 auf 16 Elemente reduziert.

4. **Vereinfachte Light-Berechnung**: `calculateSimpleLight()` ohne Schatten, max. 3 Lichter.

5. **Lazy Image-Loads**: Normal und Albedo werden nur bei Hit geladen.

6. **Max 8 Samples**: GI-Samples auf 8 begrenzt (vorher 16).

### Overhead

- **Zusätzliche BVH-Traversals**: GiSamples pro Pixel (ohne Shadow-Traces)
- **Memory**: +1 RGBA32F Image (~50MB bei 1920x1080)
- **Compute Time**: ~2-5ms pro Frame (abhängig von Samples)

### Empfehlungen

| Szenario         | GiSamples | Anmerkung                           |
|------------------|-----------|-------------------------------------|
| Echtzeit-Preview | 1-2       | Niedrige Qualität, hohe Performance |
| Interaktiv       | 4         | Guter Kompromiss                    |
| Quality Render   | 8-16      | Beste Qualität, niedrigere FPS      |

## Visuelle Effekte

### Was GI verbessert

- **Color Bleeding**: Farbige Oberflächen "strahlen" Farbe auf benachbarte Objekte ab
- **Ambient Occlusion**: Natürliche Verdunklung in Ecken und Spalten
- **Weichere Schatten**: Indirekte Beleuchtung füllt Schattenbereiche
- **Realistische Szenen**: Insgesamt natürlicherer Look

### Limitierungen

- **Nur One-Bounce**: Keine mehrfachen Reflexionen für Indirect Light
- **Noise**: Bei niedrigen Sample-Counts sichtbares Rauschen
- **Performance**: Kann auf schwächerer Hardware langsam sein

## Integration

### Datenfluss

```
Pass 2 (Lighting):
  litColor = directLight + shadows + specular

Pass 2B (Indirect):
  indirectColor = litColor + calculateIndirectLight()

Pass 3 (Reflections):
  Liest indirectColor statt litColor
  reflectedColor = indirectColor + reflections + refractions
```

### Aktivierung

```csharp
// In RenderSettings
EnableGi = true,
GiSamples = 4,
GiStrength = 0.5f
```

## Zukünftige Erweiterungen

- **Multi-Bounce GI**: Mehr als ein Bounce für akkuratere Ergebnisse
- **Temporal Accumulation**: Samples über mehrere Frames akkumulieren
- **Denoising**: AI-basiertes oder Edge-Aware Denoising
- **Light Probes**: Vorberechnete GI für statische Szenen
- **Screen-Space GI**: Hybrid-Ansatz mit Screen-Space Daten

---

**Datum**: 2026-01-31  
**Status**: ✅ Implementiert
