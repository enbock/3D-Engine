# Performance Optimierungen

## Übersicht

Dieses Dokument beschreibt die wichtigsten Performance-Optimierungen in der Vulkan Raytracing Engine.

## Shader-Optimierungen

### 1. Stack-Größen reduziert

| Pass         | Vorher | Nachher | Ersparnis    |
|--------------|--------|---------|--------------|
| Pass 1       | 64     | 32      | 50% Register |
| Pass 2       | 32     | 24      | 25% Register |
| Pass 2B (GI) | 32     | 16      | 50% Register |

Kleinere Stacks bedeuten weniger Register-Druck und bessere GPU-Auslastung.

### 2. Poisson Disk reduziert

- **Vorher**: 16 Samples
- **Nachher**: 8 Samples
- **Effekt**: Max. Shadow-Samples auf 8 begrenzt

### 3. Triangle Intersection optimiert

```glsl
// Vorher: Struct-basiert mit Hit-Objekt
Hit intersectTriangleShadow(Ray ray, Triangle tri) {
    Hit h;
    h.hit = false;
    // ...
    return h;
}

// Nachher: Inline bool für Shadow-Rays
bool intersectTriangleShadow(vec3 origin, vec3 dir, Triangle tri, float maxDist) {
    // Direkte Berechnung ohne Struct-Overhead
    return t > EPSILON && t < maxDist;
}
```

### 4. Lazy Image Loads

```glsl
// Vorher: Alle Images immer laden
vec4 posData = imageLoad(gPosition, pixelCoords);
vec4 normalData = imageLoad(gNormal, pixelCoords);
vec4 albedoData = imageLoad(gAlbedo, pixelCoords);
vec4 rayDirData = imageLoad(gRayDir, pixelCoords);

// Nachher: Nur bei Bedarf laden
vec4 posData = imageLoad(gPosition, pixelCoords);
float isHit = posData.w;

if (isHit > 0.5) {
vec4 normalData = imageLoad(gNormal, pixelCoords);
vec4 albedoData = imageLoad(gAlbedo, pixelCoords);
// ...
}
```

### 5. GI ohne Shadow-Traces

Der größte Performance-Gewinn für Pass 2B:

```glsl
// Vorher: Shadow-Trace pro Licht und Sample
vec3 calculateDirectLight(...) {
    bool inShadow = traceShadowBVH(...); // TEUER!
    // ...
}

// Nachher: Einfache Light-Berechnung
vec3 calculateSimpleLight(...) {
    // Keine Shadow-Traces für Indirect Light
    float diff = max(dot(normal, lightDir), 0.0);
    return albedo * lightColor * diff * intensity;
}
```

### 6. GI Max-Distanz reduziert

- **Vorher**: `MAX_DIST = 100.0`
- **Nachher**: `GI_MAX_DIST = 15.0`
- **Effekt**: Frühere BVH-Terminierung für GI-Rays

## Preset-Optimierungen

### Vorher

| Preset      | ShadowSamples | GiSamples |
|-------------|---------------|-----------|
| Default     | 12            | 4         |
| Performance | 8             | 2         |
| Quality     | 16            | 8         |

### Nachher

| Preset      | ShadowSamples | GiSamples |
|-------------|---------------|-----------|
| Default     | 8             | 4         |
| Performance | 4             | 2         |
| Quality     | 8             | 4         |

## Erwartete Performance-Gewinne

| Optimierung               | Geschätzter Gewinn |
|---------------------------|--------------------|
| Reduzierte Stack-Größen   | ~5-10%             |
| Reduzierte Poisson Disk   | ~10-20%            |
| Inline Triangle Intersect | ~5%                |
| Lazy Image Loads          | ~5-10%             |
| GI ohne Shadows           | ~200-400%          |
| GI reduzierte Distanz     | ~20-30%            |

## Weitere Optimierungsmöglichkeiten

### Noch nicht implementiert

1. **Temporal Accumulation**
    - GI über mehrere Frames akkumulieren
    - 1 Sample pro Frame, Ergebnis glätten

2. **Half-Resolution GI**
    - GI bei halber Auflösung berechnen
    - Upscaling mit Edge-Aware Filter

3. **Stochastic Shadows**
    - 1 Shadow-Sample pro Frame
    - Temporal Filtering

4. **Frustum Culling im BVH**
    - Nodes außerhalb des Frustums skippen

5. **SIMD-freundlichere Datenstrukturen**
    - SOA statt AOS für Triangles
    - Bessere Cache-Nutzung

6. **Early-Z Rejection**
    - Depth-Prepass für Primary Rays

## Hardware-spezifische Tipps

### NVIDIA

- Workgroup-Größe 16x16 optimal
- Shared Memory für Kamera-Daten nutzen (bereits implementiert)

### AMD

- Eventuell 8x8 Workgroups testen
- Wave64-Modus beachten

---

**Datum**: 2026-01-31
**Status**: Implementiert
