# Poisson Disk Sampling für weiche Schatten

**Datum:** 2026-01-29

## Problem

Die bisherigen weichen Schatten verwendeten zufällige Samples (`random()` Funktion), was zu mehreren Problemen führte:

- Clustering: Samples klumpten zusammen
- Sichtbare Muster bei mittleren Sample-Zahlen
- Transition von smooth → Muster → 3 Schatten → harter Schatten
- Inkonsistente Qualität je nach Shadow Sample Count

## Lösung: Poisson Disk Sampling

### Was ist Poisson Disk Sampling?

Poisson Disk Sampling ist eine Methode zur gleichmäßigen Verteilung von Punkten in einem Raum, bei der:

- Kein Punkt näher als ein Mindestabstand zu einem anderen liegt
- Die Punkte den Raum gleichmäßig ausfüllen
- Keine Cluster oder Muster entstehen

### Vorteile

1. **Konsistente Qualität**: Gleichmäßige Samples bei jeder Sample-Anzahl
2. **Keine Artefakte**: Keine Musterbildung oder Clustering
3. **Bessere Performance**: Keine teuren Random-Berechnungen pro Sample
4. **Vorhersagbare Qualität**: Linear skalierend mit Sample-Count

### Implementierung

#### Vorberechnete Poisson Disk Samples

64 vorberechnete 2D-Punkte im Einheitskreis:

```glsl
const vec2 poissonDisk[64] = vec2[](
    vec2(-0.613392, 0.617481),
    vec2(0.170019, -0.040254),
    // ... 62 weitere Punkte
);
```

#### Shadow Sampling

```glsl
for (int i = 0; i < numSamples; i++) {
    vec2 offset = poissonDisk[i];  // Gleichmäßig verteilt
    vec3 sampleDir = normalize(lightDir + (tangent * offset.x + bitangent * offset.y) * settings.shadowSoftness);
    // Shadow ray tracing...
}
```

### Technische Details

- **Maximum Samples**: 64 (mehr sind selten nötig)
- **Verteilung**: Blue-noise ähnliche Eigenschaften
- **Deterministisch**: Immer gleiche Samples für gleiche Settings
- **Rotation per Pixel**: Kann optional hinzugefügt werden für zusätzliche Variation

### Quellen

- NVIDIA: Percentage-Closer Soft Shadows (PCSS)
- Poisson Disk sampling für PCF Shadow Maps
- Blue-noise Sampling Techniken

## Ergebnis

✅ Smooth Schatten bei allen Sample-Counts
✅ Keine Muster oder Artefakte
✅ Bessere Performance (keine Random-Berechnungen)
✅ Vorhersagbare Skalierung der Qualität

## Alternative Methoden (nicht implementiert)

1. **Stratified Sampling**: Grid-basiert, kann zu Mustern führen
2. **PCSS (Percentage-Closer Soft Shadows)**: Anpassung der Softness basierend auf Blocker-Distanz
3. **Rotated Poisson Disk**: Rotation per Pixel für mehr Variation (bei Kostenim von Performance)

## Dateien geändert

- `Infrastructure/Vulkan/Shaders/raytracing.comp`
    - Entfernt: `random()` und `randomInUnitDisk()`
    - Hinzugefügt: `poissonDisk[64]` Array
    - Geändert: `traceShadow()` verwendet jetzt Poisson Disk Samples
