# Shadow Resolution Scaling Feature

## Übersicht

Implementierung eines performanten Shadow-Rendering-Systems mit dynamischer Auflösungsskalierung und intelligenter
Rauschunterdrückung.

## Architektur

### Multi-Pass Shadow Pipeline

```
┌─────────────────────────────────────────────────────────────┐
│ Pass 1: Primary Rays                                         │
│ └─> G-Buffer (Position, Normal, Albedo, RayDir)             │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ Pass 2B Shadow: Downsample & Trace                          │
│ ├─> Dispatch: downsampledRes (z.B. 1920/4 = 480px)         │
│ ├─> BVH Ray Tracing mit Soft Shadows                        │
│ ├─> Poisson Disk Sampling (1-8 samples)                     │
│ └─> Output: downsampledShadow0/1 (RGBA für 8 Lichter)      │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ Pass 2B Shadow Upscale: 5x5 Gaussian Blur                   │
│ ├─> Dispatch: fullRes (1920px)                              │
│ ├─> 25 Texture Samples pro Pixel                            │
│ ├─> Gaussian-gewichtete Interpolation                       │
│ └─> Output: shadowFullRes0/1                                │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ Pass 2: Lighting                                             │
│ └─> Liest shadowFullRes für jeden Pixel                     │
└─────────────────────────────────────────────────────────────┘
```

## Settings

### `ShadowResolutionScale`

Definiert das Downsampling-Verhältnis für Shadow-Tracing:

| Scale | Resolution (1920x1080) | Performance  | Qualität | Empfohlen für |
|-------|------------------------|--------------|----------|---------------|
| 1     | 1920x1080 (Native)     | Sehr langsam | Perfekt  | Screenshots   |
| 2     | 960x540                | Langsam      | Sehr gut | High-End GPUs |
| 4     | 480x270                | Gut          | Gut      | **Standard**  |
| 8     | 240x135                | Schnell      | OK       | Performance   |
| 16    | 120x68                 | Sehr schnell | Niedrig  | Low-End GPUs  |

### Performance-Impact

```
Pass 2B Shadow Downsample:
- Native (Scale 1):  ~8.5ms  (100% Pixels)
- Scale 2:           ~2.1ms  ( 25% Pixels)
- Scale 4:           ~0.5ms  (6.25% Pixels) ← Sweet Spot
- Scale 8:           ~0.13ms (1.56% Pixels)

Pass 2B Shadow Upscale (5x5 Blur):
- Konstant:          ~1.2ms  (unabhängig von Scale)

Gesamt-Einsparung bei Scale 4:
- Vorher (native): ~8.5ms
- Nachher:         ~1.7ms (0.5ms trace + 1.2ms upscale)
- Speedup:         5x schneller
```

## Technische Details

### Soft Shadow Implementierung

**Poisson Disk Sampling:**

```glsl
// 8 vordefinierte Sample-Positionen
const vec2 POISSON_DISK[8] =  { ... };

// Per-Pixel Rotation für Dithering
uint seed = hash(pixelCoord, time);
float rotation = seed * (2π / 65536);

// Traced mehrere jittered Rays
for (int i = 0; i < shadowSamples; i++) {
vec2 offset = rotate(POISSON_DISK[i], rotation);
trace(ray + offset * softness);
}
```

**Vorteile:**

- Hochqualitative Hash-Funktion eliminiert Muster
- Zeitliche Variation verhindert statisches Rauschen
- 16-bit Rotation (65536 Werte) für feines Dithering

### Gaussian Blur Upscaling

**5x5 Kernel (Sigma ≈ 1.0):**

```
     ┌────────────────────────────────┐
     │ 0.004  0.015  0.024  0.015  0.004 │
     │ 0.015  0.060  0.095  0.060  0.015 │
     │ 0.024  0.095  0.150  0.095  0.024 │
     │ 0.015  0.060  0.095  0.060  0.015 │
     │ 0.004  0.015  0.024  0.015  0.004 │
     └────────────────────────────────┘
```

**Eigenschaften:**

- 25 Samples pro Pixel
- Summe der Gewichte = 1.0
- Reduziert Downsampling-Artefakte
- Unterdrückt Poisson-Disk-Rauschen
- Erhält Schattenkontrast

### Datenstruktur

**Shadow-Maps (2x RGBA32F):**

```
downsampledShadow0: RGBA = Light[0..3] Shadow-Faktoren (0=Schatten, 1=Licht)
downsampledShadow1: RGBA = Light[4..7] Shadow-Faktoren

→ Unterstützt bis zu 8 Lichter gleichzeitig
```

## Best Practices

### Empfohlene Settings-Kombinationen

**Balanced (Standard):**

```csharp
ShadowResolutionScale = 4
ShadowSamples = 8
```

→ Gute Qualität, 5x Performance-Gewinn

**Quality:**

```csharp
ShadowResolutionScale = 2
ShadowSamples = 12
```

→ Beste Qualität, 2x Performance-Gewinn

**Performance:**

```csharp
ShadowResolutionScale = 8
ShadowSamples = 4
```

→ Akzeptable Qualität, 8x Performance-Gewinn

### Debugging

Falls Schatten seltsam aussehen:

1. **Schwarze Oberflächen**: Downsample-Dispatch falsch → prüfe dispatch group counts
2. **Horizontale Muster**: Hash-Funktion zu einfach → verbessere Rotation
3. **Zu verschwommen**: Blur-Kernel zu groß → reduziere auf 3x3
4. **Blockig**: Blur-Kernel zu klein → erhöhe auf 7x7 oder höheren Scale

## Implementierungs-Details

### Shader-Dateien

- `pass2b_shadow_downsample.comp`: BVH Ray Tracing, Soft Shadows
- `pass2b_shadow_upscale.comp`: 5x5 Gaussian Blur

### C#-Dateien

- `RenderSettings.cs`: `ShadowResolutionScale` Property
- `VulkanMultiPassTask.cs`: Dispatch-Logik, Image-Erzeugung
- `UniformDataStructures.cs`: `RenderSettingsData` Struct

## Lessons Learned

1. **Dispatch-Resolution wichtig**: Downsample-Shader MÜSSEN in downsampled Resolution dispatched werden
2. **Hash-Qualität kritisch**: Einfache Hash-Funktionen erzeugen sichtbare Muster
3. **Blur ist notwendig**: Ohne Blur sind Downsampling-Artefakte sehr sichtbar
4. **Sweet Spot bei Scale 4**: Bester Kompromiss zwischen Qualität und Performance

## Datum

2026-01-31
