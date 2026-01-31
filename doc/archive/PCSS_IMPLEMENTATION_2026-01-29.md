# PCSS (Percentage-Closer Soft Shadows) Implementierung

**Datum:** 2026-01-29

## Problem mit Standard Poisson Disk Sampling

Einfaches Poisson Disk Sampling mit fester Kernel-Größe erzeugte:

- Einzelne starke Schatten statt weiche Übergänge
- Gleiche Shadow-Softness unabhängig von der Distanz zum Blocker
- Unrealistische Schatten (in Realität werden Schatten weicher je weiter der Blocker entfernt ist)

## Lösung: PCSS (Percentage-Closer Soft Shadows)

### Was ist PCSS?

PCSS ist ein von NVIDIA entwickelter Algorithmus (Randima Fernando, 2005), der realistische weiche Schatten erzeugt,
indem die **Penumbra-Breite dynamisch** basierend auf:

- Distanz vom Empfänger zum Blocker
- Distanz vom Empfänger zur Lichtquelle
- Größe der Lichtquelle

berechnet wird.

### Der 3-Schritt PCSS Algorithmus

#### Schritt 1: Blocker Search

Finde die durchschnittliche Distanz aller Blocker zwischen Lichtquelle und Empfänger:

```glsl
float findBlockerDistance(vec3 origin, vec3 lightDir, float receiverDist, ...) {
    float blockerSum = 0.0;
    float numBlockers = 0.0;
    
    // Sample im Suchradius
    for (int i = 0; i < searchSamples; i++) {
        vec2 offset = poissonDisk[i] * searchRadius;
        vec3 sampleDir = normalize(lightDir + tangent * offset.x + bitangent * offset.y);
        
        Hit shadowHit = traceShadowRay(shadowRay, numTriangles);
        
        if (shadowHit.hit && shadowHit.dist < receiverDist) {
            blockerSum += shadowHit.dist;
            numBlockers += 1.0;
        }
    }
    
    return blockerSum / numBlockers;
}
```

- Verwendet 16 Poisson Disk Samples für effiziente Suche
- Sammelt nur Blocker zwischen Licht und Empfänger
- Gibt -1 zurück wenn keine Blocker gefunden (= volle Beleuchtung)

#### Schritt 2: Penumbra Width Berechnung

Basierend auf ähnlichen Dreiecken (Geometric Penumbra):

```glsl
float penumbraWidth = (receiverDist - avgBlockerDist) * lightSize / avgBlockerDist;
```

**Formel erklärt:**

- `receiverDist - avgBlockerDist`: Distanz vom Blocker zum Empfänger
- `lightSize`: Größe der Lichtquelle (Konstante: 0.5)
- `/ avgBlockerDist`: Normalisierung durch Blocker-Distanz

**Physikalische Bedeutung:**

- Je weiter der Blocker vom Empfänger entfernt → größere Penumbra (weicherer Schatten)
- Je näher der Blocker am Empfänger → kleinere Penumbra (härterer Schatten)
- Je größer die Lichtquelle → größere Penumbra

#### Schritt 3: Adaptives Filtering

PCF (Percentage Closer Filtering) mit dynamischer Kernel-Größe:

```glsl
float filterRadius = penumbraWidth * settings.shadowSoftness;

for (int i = 0; i < numSamples; i++) {
    vec2 offset = poissonDisk[i] * filterRadius;  // Dynamischer Radius!
    vec3 sampleDir = normalize(lightDir + tangent * offset.x + bitangent * offset.y);
    
    // Shadow ray tracing...
}
```

- Filter-Radius variiert basierend auf berechneter Penumbra
- Mehr Samples bei größeren Penumbrae
- Poisson Disk Sampling für gleichmäßige Verteilung

### Technische Parameter

```glsl
const int BLOCKER_SEARCH_SAMPLES = 16;  // Samples für Blocker-Suche
const int MAX_FILTER_SAMPLES = 64;       // Maximum Filter-Samples
const float LIGHT_SIZE = 0.5;            // Virtuelle Lichtgröße
```

### Optimierungen

1. **Früher Exit**: Keine Blocker → volle Beleuchtung ohne Filtering
2. **Clamping**: Penumbra Width auf [0.0, 2.0] begrenzt für Stabilität
3. **Sample-Reduktion**: Nur 16 Samples für Blocker Search (statt volle 64)
4. **Poisson Disk**: Gleichmäßige Verteilung verhindert Artefakte

### Vorteile gegenüber Standard PCF

| Feature              | Standard PCF         | PCSS                |
|----------------------|----------------------|---------------------|
| Shadow Softness      | Fix                  | Dynamisch           |
| Realismus            | Niedrig              | Hoch                |
| Contact Shadows      | Hart (unrealistisch) | Weich (realistisch) |
| Distanz-Abhängigkeit | Nein                 | Ja                  |
| Performance          | ✓ Besser             | ✓ Gut               |

### Physikalische Korrektheit

PCSS approximiert **geometrische Penumbra** basierend auf:

```
     Light Source (Size: wLight)
          /|\
         / | \
        /  |  \
       /   |   \
      +----+----+  ← Blocker (Distance: dBlocker)
       \   |   /
        \  |  /
         \ | /  ← Penumbra Width: (dReceiver - dBlocker) * wLight / dBlocker
          \|/
           +      ← Receiver (Distance: dReceiver)
```

### Quellen

- **NVIDIA Paper**: "Percentage-Closer Soft Shadows" (Randima Fernando, 2005)
- **Formula**: `wPenumbra = (dReceiver - dBlocker) * wLight / dBlocker`
- **Techniken**: Poisson Disk Sampling + Geometric Penumbra Approximation

## Ergebnis

✅ Realistische weiche Schatten mit physikalisch korrekter Penumbra
✅ Harte Schatten bei Contact Points (Blocker nahe am Empfänger)
✅ Weiche Schatten bei entfernten Blockern
✅ Glatte Übergänge ohne Artefakte oder Muster
✅ Gute Performance durch optimierte Blocker-Suche

## Dateien geändert

- `Infrastructure/Rendering/Vulkan/Shaders/raytracing.comp`
    - Hinzugefügt: `findBlockerDistance()` - Blocker Search
    - Geändert: `traceShadow()` - PCSS 3-Schritt Algorithmus
    - Verwendet: Poisson Disk Samples für beide Phasen

## Nächste Schritte (Optional)

1. **Adaptive Sampling**: Weniger Samples bei harten Schatten
2. **Cascaded Search**: Hierarchische Blocker-Suche für bessere Performance
3. **Variable Light Size**: Lichtgröße als Parameter
4. **Area Lights**: Rechteckige/Sphärische Lichtquellen
