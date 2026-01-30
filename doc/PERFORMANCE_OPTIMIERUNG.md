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
| MaxBounces | 3 | 2 |
| Reflections | true | true |
| ShadowSamples | 4 | 1 |
| ReflectionStrength | 0.5 | 0.4 |

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

### 4. BVH-Integration vollständig implementiert ✅

#### CPU-Seite:

- `BvhBuilderService` mit rekursivem Aufbau
- `FlatBvhNode` Struktur für GPU-Transfer
- Median-Split Strategie mit Sortierung nach längster Achse
- Max 4 Dreiecke pro Leaf-Node

#### GPU-Seite:

- `BvhNodeData` Struktur in `UniformDataStructures.cs`
- Binding 5 für BVH-Buffer in Descriptor Layout
- BVH-Buffer Erstellung in `VulkanBufferHelper`

#### Shader:

- `intersectAABB()` - Ray-Box Intersection
- `traceBVH()` - Stack-basierte BVH Traversal für Primary Rays
- `traceShadowBVH()` - BVH Traversal für Shadow Rays
- Alle Lichtberechnungen nutzen jetzt BVH

#### Komplexität:

- **Vorher:** O(n) - Linear durch alle Dreiecke
- **Nachher:** O(log n) - Nur relevante Teilbäume

---

## Geplante Optimierungen

### Phase 1: Schnelle Wins (erledigt ✅)

- [x] Performance-Preset verwenden
- [x] Ultra-Performance-Preset erstellen

### Phase 2: BVH-Integration (erledigt ✅)

- [x] BVH auf CPU bauen (`BvhBuilderService`)
- [x] BVH-Buffer an GPU senden
- [x] Shader mit BVH-Traversal aktualisieren
- [x] Shadow-Rays über BVH
- [x] Primary-Rays über BVH
- [x] Reflection-Rays über BVH

### Phase 3: Shader-Optimierungen (erledigt ✅)

1. **Shared Memory für Camera-Daten**
    - Camera-Position, Forward, Right, Up Vektoren
    - tanHalfFov und AspectRatio vorberechnet
    - Nur Thread 0 berechnet, alle anderen lesen aus Shared Memory
    - Spart redundante Berechnungen pro Pixel

2. **Optimierte AABB-Intersection**
    - invDir wird einmal pro Ray berechnet, nicht pro AABB-Test
    - Reduziert Divisionen drastisch

3. **Entfernung ungenutzter Code**
    - Alte lineare trace() und traceShadow() Funktionen entfernt
    - Kleinerer Shader = schnellere Kompilierung und Ausführung

4. **Stack-Größen optimiert**
    - Primary Rays: Stack[64] (tiefe BVH-Bäume)
    - Shadow Rays: Stack[32] (früher Abbruch wahrscheinlich)

---

## Performance-Erwartungen

| Optimierung          | Erwarteter Speedup          | Status |
|----------------------|-----------------------------|--------|
| Performance-Preset   | 2-4x                        | ✅      |
| BVH-Integration      | 10-50x (abhängig von Szene) | ✅      |
| Shader-Optimierungen | 1.2-1.5x                    | ✅      |

### BVH-Performance-Analyse

Bei 450 Dreiecken:

- **Ohne BVH:** 450 Intersection-Tests pro Ray
- **Mit BVH:** ~9 Intersection-Tests pro Ray (log₂(450) ≈ 9)
- **Theoretischer Speedup:** ~50x

Bei 10.000 Dreiecken:

- **Ohne BVH:** 10.000 Intersection-Tests pro Ray
- **Mit BVH:** ~14 Intersection-Tests pro Ray
- **Theoretischer Speedup:** ~700x

---

## Nächste Schritte

Alle geplanten Optimierungen sind implementiert. Mögliche zukünftige Verbesserungen:

1. **SAH (Surface Area Heuristic) für BVH-Aufbau** - bessere Baumqualität
2. **Morton-Code Sortierung** - cache-freundlichere Speicheranordnung
3. **Instancing** - gleiche Geometrie mehrfach ohne Kopieren
4. **Level of Detail (LOD)** - entfernte Objekte mit weniger Dreiecken

## Konfiguration

In `VulkanBufferHelper.cs`:

```csharp
// Für beste Performance:
RenderSettings settings = RenderSettings.UltraPerformance;

// Für Balance:
RenderSettings settings = RenderSettings.Performance;

// Für Qualität:
RenderSettings settings = RenderSettings.Default;
```

**Hinweis:** Single-Pass Rendering wurde zugunsten von Multi-Pass entfernt.
Siehe [MULTI_PASS_STRATEGIE.md](./MULTI_PASS_STRATEGIE.md) für Details.
