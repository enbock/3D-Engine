# Performance-Optimierung der Ray-Tracing Engine

## Analyse der Performance-Probleme

### Aktueller Stand

- ~450 Polygone → <20 FPS
- 4-Pass Rendering Pipeline (Primary, Lighting, Reflections, Composite)

### Hauptprobleme identifiziert

#### 1. Lineare Dreiecks-Suche O(n)

**Problem:** Alle Shader durchlaufen alle Dreiecke in einer Schleife:

```glsl
for (int i = 0; i < numTriangles; i++) {
Hit h = intersectTriangle(ray, triangles[i]);
...
}
```

**Bei 450 Dreiecken pro Pixel:**

- Pass 1 (Primary): 450 Intersections
- Pass 2 (Lighting): 450 × Anzahl Lichter × Shadow-Rays
- Pass 3 (Reflections): 450 × Bounces × Shadow-Rays

Bei 1920x1080 = 2.073.600 Pixel bedeutet das:

- Primary: 933 Millionen Intersection-Tests
- Mit Shadows: mehrere Milliarden Tests pro Frame

**Lösung:** BVH (Bounding Volume Hierarchy) → O(log n)

#### 2. Multi-Pass Overhead

- 4 separate Compute-Passes statt 1
- Zusätzliche Image-Reads/-Writes zwischen Passes
- 307 MB VRAM extra für G-Buffers

#### 3. RenderSettings Default zu teuer

- 3 Bounces mit Reflections
- 4 Shadow-Samples
- Reflections aktiviert (teuerster Pass)

---

## Implementierte Optimierungen

### 1. Single-Pass Rendering aktiviert ✅

```csharp
// EngineConfig.cs
public static bool UseMultiPassRendering => false;
```

**Vorteil:** Kein Multi-Pass Overhead, weniger VRAM

### 2. Performance-Preset aktiviert ✅

```csharp
// VulkanBufferHelper.cs
RenderSettings settings = RenderSettings.Performance;
```

Änderungen:
| Setting | Default | Performance |
|---------|---------|-------------|
| MaxBounces | 3 | 1 |
| Reflections | true | false |
| ShadowSamples | 4 | 1 |
| ReflectionStrength | 0.5 | 0.0 |

### 3. Ultra-Performance-Preset erstellt ✅

```csharp
public static RenderSettings UltraPerformance => new()
{
    MaxBounces = 0,
    EnableShadows = false,
    EnableReflections = false,
    ...
};
```

### 4. BVH-Infrastruktur vorbereitet ✅

- `BvhNodeData` Struktur für GPU erstellt
- `BvhBuilderService` für CPU-seitigen Aufbau erstellt
- `raytracing_bvh.comp` Shader mit BVH-Traversal vorbereitet

---

## Geplante Optimierungen

### Phase 1: Schnelle Wins (erledigt ✅)

- [x] Performance-Preset verwenden
- [x] Ultra-Performance-Preset erstellen

### Phase 2: BVH-Integration (geplant)

1. BVH auf CPU bauen
2. BVH-Buffer an GPU senden
3. Shader mit BVH-Traversal aktualisieren

### Phase 3: Shader-Optimierungen (geplant)

1. Early-Exit bei Shadow-Rays verbessern
2. Frustum Culling vor Ray-Trace
3. Shared Memory für häufig verwendete Daten

---

## Performance-Erwartungen

| Optimierung          | Erwarteter Speedup          |
|----------------------|-----------------------------|
| Performance-Preset   | 2-4x                        |
| BVH-Integration      | 10-50x (abhängig von Szene) |
| Shader-Optimierungen | 1.2-1.5x                    |

---

## Nächste Schritte

1. Testen ob Performance-Preset ausreicht
2. Bei Bedarf: Single-Pass statt Multi-Pass
3. BVH-Integration wenn >1000 Dreiecke benötigt

## Konfiguration umschalten

In `VulkanBufferHelper.cs`:

```csharp
// Für beste Performance:
RenderSettings settings = RenderSettings.UltraPerformance;

// Für Balance:
RenderSettings settings = RenderSettings.Performance;

// Für Qualität:
RenderSettings settings = RenderSettings.Default;
```

In `EngineConfig.cs`:

```csharp
// Single-Pass ist schneller bei einfachen Szenen
public static bool UseMultiPassRendering = false;
```
