# HDR und Tone Mapping Feature

## Übersicht

Das HDR (High Dynamic Range) Feature ermöglicht eine realistische Darstellung von Licht und Farben mit erweitertem
Dynamikbereich. Die Implementierung unterstützt **echte 10-bit Ausgabe** für HDR-Monitore und verschiedene
Tone-Mapping-Operatoren.

## 10-Bit Ausgabe (HDR-Monitor)

### Aktivierung

In `EngineConfig`:

```csharp
public bool EnableHdr10 { get; init; } = true;
public float HdrMinNits { get; init; } = 0.0f;
public float HdrMaxNits { get; init; } = 400.0f;
```

### HDR Nits Konfiguration

| Wert           | Beschreibung                                    |
|----------------|-------------------------------------------------|
| **HdrMinNits** | Minimale Helligkeit (Schwarzpunkt), typisch 0.0 |
| **HdrMaxNits** | Peak-Helligkeit für weißes Bild                 |

Empfohlene `HdrMaxNits` Werte:

- **200-300**: Dunkleres, cinematisches Bild
- **400-500**: Normales, ausgewogenes Bild (Standard)
- **600-800**: Helleres, lebhafteres Bild
- **1000+**: Für HDR-Monitore mit hoher Spitzenhelligkeit

### Unterstützte Formate (Priorität)

1. **HDR10 ST2084** - `A2B10G10R10UnormPack32` mit `Hdr10St2084` Farbraum
    - Echtes HDR für HDR10-Monitore
    - PQ (Perceptual Quantizer) Transfer-Funktion

2. **10-bit sRGB** - `A2B10G10R10UnormPack32` mit sRGB Farbraum
    - 10-bit Farbtiefe ohne HDR-Metadaten
    - Weniger Banding in Gradienten

3. **16-bit Float** - `R16G16B16A16Sfloat`
    - Höchste Präzision
    - Für professionelle Anwendungen

4. **8-bit sRGB** (Fallback) - `B8G8R8A8Srgb`
    - Standard SDR-Ausgabe

## Tone-Mapping-Operatoren

### 1. None (Kein Tone Mapping)

- Einfaches Clamping auf [0, 1]
- Schnellste Option
- Für einfache Szenen ohne HDR-Beleuchtung

### 2. Reinhard

- Klassischer Tone-Mapping-Operator
- Formel: `color / (1 + color)`
- Weiche Übergänge
- Gut für Outdoor-Szenen

### 3. ACES Filmic (Standard)

- Academy Color Encoding System
- Filmischer Look
- Guter Kontrast in Schatten und Highlights
- Empfohlen für die meisten Szenen

### 4. Uncharted 2

- Basiert auf Naughty Dogs Implementierung
- Hoher Kontrast
- Gut für dunkle Szenen mit hellen Highlights

## Konfiguration

Die HDR-Einstellungen werden über `RenderSettings` konfiguriert:

```csharp
public class RenderSettings
{
    public bool EnableHdr { get; private init; } = true;
    public float Exposure { get; private init; } = 1.0f;
    public float Gamma { get; private init; } = 2.2f;
    public ToneMappingOperator ToneMapping { get; private init; } = ToneMappingOperator.AcesFilmic;
}
```

### Parameter

| Parameter   | Typ   | Standard   | Beschreibung                            |
|-------------|-------|------------|-----------------------------------------|
| EnableHdr   | bool  | true       | Aktiviert HDR-Rendering                 |
| Exposure    | float | 1.0        | Belichtungsstärke (0.1 - 3.0 empfohlen) |
| Gamma       | float | 2.2        | Gamma-Korrektur für sRGB                |
| ToneMapping | enum  | AcesFilmic | Tone-Mapping-Operator                   |

## Presets

### Quality

- EnableHdr: true
- Exposure: 1.2
- ToneMapping: ACES Filmic

### Default

- EnableHdr: true
- Exposure: 1.0
- ToneMapping: ACES Filmic

### Performance

- EnableHdr: true
- Exposure: 1.0
- ToneMapping: Reinhard

### UltraPerformance

- EnableHdr: false
- ToneMapping: None

## Shader-Implementierung

Das HDR-Processing erfolgt im `pass4_composite.comp` Shader:

1. **Tone Mapping** - Konvertiert HDR-Werte in LDR
2. **Gamma-Korrektur** - Passt Farben für sRGB-Anzeige an

### Tone-Mapping-Formeln

**Reinhard:**

```glsl
color * exposure / (1.0 + color * exposure)
```

**ACES Filmic:**

```glsl
(color * (2.51 * color + 0.03)) / (color * (2.43 * color + 0.59) + 0.14)
```

**Uncharted 2:**

```glsl
((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F
```

### PQ (ST.2084) Encoding für HDR10

Bei HDR10-Ausgabe wird nach dem Tone Mapping die **Perceptual Quantizer** Kurve angewendet:

```glsl
vec3 linearToST2084(vec3 color) {
    float m1 = 0.1593017578125;
    float m2 = 78.84375;
    float c1 = 0.8359375;
    float c2 = 18.8515625;
    float c3 = 18.6875;

    vec3 Y = color * (SDR_NITS / 10000.0);  // SDR_NITS = 80
    vec3 Ym1 = pow(Y, vec3(m1));
    return pow((c1 + c2 * Ym1) / (1.0 + c3 * Ym1), vec3(m2));
}
```

Dies konvertiert lineare Farbwerte in das PQ-kodierte Format, das HDR10-Monitore erwarten.

## Technische Details

### Swapchain-Konfiguration

Die Engine wählt automatisch das beste verfügbare HDR-Format:

- `VulkanSwapchainTask.CreateSwapchain()` prüft verfügbare Surface-Formate
- Fallback-Kette: HDR10 → 10-bit sRGB → 16-bit Float → 8-bit sRGB

### Storage Image Format

Bei aktiviertem HDR:

- Format: `A2B10G10R10UnormPack32` (10-bit pro RGB-Kanal)
- Layout: `rgb10_a2` im Shader

### Shader-Binding (pass4_composite.comp)

- Binding 0: reflectedColor (HDR Input)
- Binding 1: outputImage (10-bit oder 8-bit Output)
- Binding 2: CameraUBO
- Binding 3: RenderSettings (mit HDR-Parametern)

### RenderSettingsData Struktur

```csharp
public struct RenderSettingsData
{
    // ... bestehende Felder ...
    public int EnableHdr;
    public float Exposure;
    public float Gamma;
    public int ToneMapping;
}
```

## Kompilierung

Nach Änderungen am Shader muss dieser neu kompiliert werden:

```batch
compile_shaders.bat
```

## Zukünftige Erweiterungen

- [ ] Auto-Exposure basierend auf Szene-Luminanz
- [ ] Bloom-Effekt für helle Bereiche
- [ ] Eye Adaptation (temporale Anpassung)
- [ ] Local Tone Mapping
- [ ] HDR10 / Dolby Vision Ausgabe
