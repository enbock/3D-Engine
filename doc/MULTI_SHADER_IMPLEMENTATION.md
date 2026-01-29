# Multi-Shader System - Implementierung

## Status: ✅ IMPLEMENTIERT (2026-01-29)

## Entscheidung

Nach erneuter Analyse und Optimierung von Shader und Renderer wurde die Multi-Pass Implementierung **erfolgreich
durchgeführt**.

Siehe **[MULTI_PASS_IMPLEMENTATION_COMPLETE.md](./MULTI_PASS_IMPLEMENTATION_COMPLETE.md)** für vollständige Details.

## Ursprüngliche Bedenken (Überholt)

Die ursprünglichen Bedenken bzgl. Komplexität waren berechtigt, aber nach der Refactoring-Phase war die Implementierung
machbar:

- ✅ ~1500 Zeilen neuer Code (erwartet: 1000+)
- ✅ 6 neue Images (wie erwartet)
- ✅ 4 neue Pipelines (wie erwartet)
- ✅ 4 neue Descriptor Set Layouts (wie erwartet)
- ✅ Command Buffer Refactoring (durchgeführt)
- ✅ Resize-Logic angepasst (durchgeführt)
- ✅ ~307 MB VRAM statt 8.3 MB (wie erwartet)
- ✅ Memory Barriers implementiert (durchgeführt)

**Entwicklungszeit**: ~2 Stunden (besser als erwartet: 4-6h)
**Risiko**: Moderat (gut durch Task-Architektur abgefedert)

## Implementierte Lösung: Multi-Pass Rendering

### Architektur

**Pass 1**: Primary Rays → G-Buffer (Position, Normal, Albedo, RayDir)  
**Pass 2**: Lighting → Lit Color (mit Shadows)  
**Pass 3**: Reflections → Reflected Color (Multi-Bounce)  
**Pass 4**: Composite → Final Output (Gamma + BGR)

### Erfolgreiche Features

✅ Kleinere, fokussierte Shader (je ~150-250 Zeilen)  
✅ Klare Verantwortlichkeiten  
✅ Modulare Features (einzeln aktivierbar via Config)  
✅ Besseres Debugging (G-Buffer inspizierbar)  
✅ Toggle zwischen Single-Pass / Multi-Pass

## Alternative Lösung: Code-Organisation (Bereits durchgeführt)

Die Funktions-Aufteilung im bestehenden Shader wurde bereits im vorherigen Refactoring durchgeführt:

✅ **[SHADER_REFACTORING_COMPLETE.md](./SHADER_REFACTORING_COMPLETE.md)**

- main() von 47→16 Zeilen (-66%)
- shade() von 50→16 Zeilen (-68%)
- 7 neue spezialisierte Funktionen
- Magic Numbers durch Konstanten ersetzt

### 1. Funktions-Aufteilung im selben Shader

```glsl
// === Intersection Functions ===
Hit intersectTriangle(Ray ray, Triangle tri) { ... }
Hit intersectTriangleShadow(Ray ray, Triangle tri) { ... }
Hit trace(Ray ray, int numTriangles) { ... }
bool traceShadow(...) { ... }

// === Shading Functions ===
vec3 calculateLighting(Hit hit, vec3 rayDir, int numTriangles) { ... }
vec3 calculateShadows(vec3 origin, vec3 lightDir, ...) { ... }
vec3 calculateReflections(Hit hit, vec3 rayDir, ...) { ... }

// === Main Entry Point ===
void main() {
    Ray ray = generateRay();
    Hit hit = trace(ray, numTriangles);
    
    vec3 color = vec3(0.0);
    if (hit.hit) {
        color = calculateLighting(hit, ray.direction, numTriangles);
        color += calculateReflections(hit, ray.direction, ...);
    }
    
    imageStore(outputImage, pixelCoords, vec4(color, 1.0));
}
```

**Vorteile**:

- ✅ Gleiche Performance wie aktuell
- ✅ Keine C# Änderungen nötig
- ✅ Bessere Lesbarkeit durch klare Funktionsnamen
- ✅ Kein Memory Overhead
- ✅ Sofort umsetzbar (30 Minuten statt 6 Stunden)

### 2. Kommentare und Regionen

```glsl
// ═══════════════════════════════════════
// INTERSECTION
// ═══════════════════════════════════════

// ...intersection functions...

// ═══════════════════════════════════════
// SHADING
// ═══════════════════════════════════════

// ...shading functions...
```

### 3. Conditional Compilation für Features

```glsl
#define ENABLE_REFLECTIONS 1
#define ENABLE_SHADOWS 1
#define MAX_BOUNCES 3

#if ENABLE_REFLECTIONS
    color += calculateReflections(...);
#endif
```

## Empfehlung

**Phase 1** (JETZT): Funktions-Refactoring des bestehenden Shaders

- Kleine, fokussierte Funktionen
- Klare Kommentare
- Gleiche Performance

**Phase 2** (SPÄTER): Multi-Pass nur wenn wirklich nötig

- z.B. für Denoising
- z.B. für Temporal Anti-Aliasing
- z.B. für Post-Processing-Effekte

**Begründung**:

- "Shader-Komplexität reduzieren" != "Multi-Pass Rendering"
- Funktionen + Kommentare sind ausreichend
- Multi-Pass lohnt sich erst bei >500 Zeilen pro Shader

## Ziel

Aufteilung des monolithischen `raytracing.comp` Shaders in mehrere spezialisierte Shader:

1. **primary_rays.comp** - Ray Generation & Scene Intersection → G-Buffer
2. **lighting.comp** - Lighting & Shadow Calculation
3. **reflections.comp** - Reflection Bounces
4. **composite.comp** - Final Composition & Post-Processing

## Vorteile

- ✅ **Kleinere Shader-Dateien** (leichter zu debuggen und zu warten)
- ✅ **Wiederverwendbare Funktionen** (Intersection, Shading)
- ✅ **Modulare Struktur** (Features können einzeln aktiviert/deaktiviert werden)
- ✅ **Bessere Lesbarkeit** (jeder Shader hat eine klare Aufgabe)

## Architektur

### Deferred Shading Ansatz

```
Pass 1: Primary Rays
Input:  Camera, Triangles
Output: G-Buffer (Position, Normal, Albedo, RayDir)

Pass 2: Lighting  
Input:  G-Buffer, Lights, Triangles, Settings
Output: Lit Color

Pass 3: Reflections
Input:  G-Buffer, Lit Color, Triangles, Lights, Settings
Output: Final Color with Reflections

Pass 4: Composite
Input:  Final Color
Output: Swapchain Image (Gamma corrected, BGR format)
```

### G-Buffer Layout

```glsl
gPosition (RGBA32F):  vec3 hitPoint + float isHit (1.0 = hit, 0.0 = sky)
gNormal   (RGBA32F):  vec3 normal + float pad
gAlbedo   (RGBA32F):  vec3 color + float pad (or skyColor + 0.0)
gRayDir   (RGBA32F):  vec3 rayDirection + float pad
```

## Implementierte Dateien

### Shader-Dateien

- ✅ `common.glsl` - Gemeinsame Strukturen (Triangle, Light, Ray, Hit, Constants)
- ✅ `intersection.glsl` - Ray-Triangle Intersection Funktionen
- ✅ `primary_rays.comp` - G-Buffer Generation
- ✅ `lighting.comp` - Lighting Pass
- ✅ `reflections.comp` - Reflection Pass
- ✅ `composite.comp` - Final Composition

### Build-Dateien

- ✅ `compile_shaders.bat` - Updated für Multi-Shader Kompilierung

## Aktuelle Probleme

### GLSL Include Problem

**Problem**: GLSL `#include` muss NACH `#version` stehen, aber glslc unterstützt das nicht standardmäßig.

**Lösung**: Direkt inline - alle Strukturen und Funktionen direkt in jedem Shader einfügen (kein #include).

### Kompilierungs-Status

- ⏳ `primary_rays.comp` - In Arbeit
- ⏳ `lighting.comp` - In Arbeit
- ⏳ `reflections.comp` - In Arbeit
- ⏳ `composite.comp` - In Arbeit

## Nächste Schritte

1. ✅ Shader-Fehler finden und beheben
2. ⏳ C# Code anpassen für Multi-Pass Rendering
3. ⏳ G-Buffer Images erstellen (4x RGBA32F)
4. ⏳ Intermediate Image erstellen (Lighting Output, Reflection Output)
5. ⏳ Descriptor Sets für jeden Pass konfigurieren
6. ⏳ Command Buffer für 4 Dispatches aufzeichnen
7. ⏳ Pipeline Barriers zwischen Pässen einfügen
8. ⏳ Testen und debuggen

## C# Änderungen (TODO)

### Neue Images

```csharp
// G-Buffer
Image _gPosition;
ImageView _gPositionView;
DeviceMemory _gPositionMemory;

Image _gNormal;
ImageView _gNormalView;
DeviceMemory _gNormalMemory;

Image _gAlbedo;
ImageView _gAlbedoView;
DeviceMemory _gAlbedoMemory;

Image _gRayDir;
ImageView _gRayDirView;
DeviceMemory _gRayDirMemory;

// Intermediate
Image _lightingOutput;
ImageView _lightingOutputView;
DeviceMemory _lightingOutputMemory;

Image _reflectionOutput;
ImageView _reflectionOutputView;
DeviceMemory _reflectionOutputMemory;
```

### Neue Pipelines

```csharp
Pipeline _primaryRaysPipeline;
Pipeline _lightingPipeline;
Pipeline _reflectionsPipeline;
Pipeline _compositePipeline;

ShaderModule _primaryRaysShader;
ShaderModule _lightingShader;
ShaderModule _reflectionsShader;
ShaderModule _compositeShader;

PipelineLayout _primaryRaysLayout;
PipelineLayout _lightingLayout;
PipelineLayout _reflectionsLayout;
PipelineLayout _compositeLayout;
```

### Neue Descriptor Sets

```csharp
DescriptorSet _primaryRaysDescriptorSet;
DescriptorSet _lightingDescriptorSet;
DescriptorSet _reflectionsDescriptorSet;
DescriptorSet _compositeDescriptorSet;
```

### Command Buffer Struktur

```csharp
// Pass 1: Primary Rays
_vk.CmdBindPipeline(cmd, PipelineBindPoint.Compute, _primaryRaysPipeline);
_vk.CmdBindDescriptorSets(..., _primaryRaysDescriptorSet, ...);
_vk.CmdDispatch(cmd, groupCountX, groupCountY, 1);
_vk.CmdPipelineBarrier(...); // G-Buffer schreib-fertig

// Pass 2: Lighting
_vk.CmdBindPipeline(cmd, PipelineBindPoint.Compute, _lightingPipeline);
_vk.CmdBindDescriptorSets(..., _lightingDescriptorSet, ...);
_vk.CmdDispatch(cmd, groupCountX, groupCountY, 1);
_vk.CmdPipelineBarrier(...); // Lighting schreib-fertig

// Pass 3: Reflections
_vk.CmdBindPipeline(cmd, PipelineBindPoint.Compute, _reflectionsPipeline);
_vk.CmdBindDescriptorSets(..., _reflectionsDescriptorSet, ...);
_vk.CmdDispatch(cmd, groupCountX, groupCountY, 1);
_vk.CmdPipelineBarrier(...); // Reflection schreib-fertig

// Pass 4: Composite
_vk.CmdBindPipeline(cmd, PipelineBindPoint.Compute, _compositePipeline);
_vk.CmdBindDescriptorSets(..., _compositeDescriptorSet, ...);
_vk.CmdDispatch(cmd, groupCountX, groupCountY, 1);
// Output ist _storageImage (wie vorher)
```

## Performance-Erwartungen

### Memory Overhead

**Vorher**: 1x Storage Image (RGBA8)
**Nachher**:

- 4x G-Buffer Images (RGBA32F) = 16x mehr
- 2x Intermediate Images (RGBA32F) = 8x mehr
- **Total**: ~24x mehr VRAM bei 2560x1440

**Bei 2560x1440**:

- Vorher: 14.7 MB
- Nachher: ~353 MB
- **Akzeptabel** für moderne GPUs (RTX 3070 hat 8 GB)

### Performance Impact

**Positive**:

- ✅ Weniger Redundanz (Intersection nur 1x)
- ✅ Bessere Shader-Occupancy (kleinere Shader)
- ✅ Potenzial für Optimierung pro Pass

**Negative**:

- ❌ Mehr Memory Bandwidth (G-Buffer read/write)
- ❌ 4x Dispatch Overhead (minimal)
- ❌ Pipeline Barriers zwischen Pässen

**Erwartung**: Leichter Performance-Verlust (~5-10%), aber bessere Wartbarkeit

## Begründung

**Warum machen wir das?**

Das ursprüngliche Ziel war **Shader-Komplexität reduzieren**, nicht Performance-Optimierung.

Der monolithische `raytracing.comp` (307 Zeilen) wird schwer wartbar bei:

- Mehr Lichttypen
- Mehr Material-Typen
- Global Illumination
- Denoising
- Anti-Aliasing

Mit Multi-Shader-System:

- Jeder Shader < 150 Zeilen
- Klare Verantwortlichkeiten
- Features können einzeln aktiviert/deaktiviert werden
- Debugging ist einfacher (welcher Pass ist falsch?)

## Lessons Learned

### GLSL Include Support

`glslc` unterstützt `-I` für Include-Pfade, ABER:

- `#include` muss nach `#version` stehen
- Das ist in GLSL erlaubt, aber ungewöhnlich
- Besser: Direkt inline oder externes Preprocessing

### Alternative Ansätze

Wenn Performance zum Problem wird:

1. **Hybrid**: Primary Rays + Lighting in einem Pass, Reflections separat
2. **Conditional Compilation**: `#define PASS_PRIMARY` pro Shader-Variante
3. **Compute Shader Libraries**: Vulkan 1.3 Feature (zu komplex)

---

**Datum**: 2026-01-29  
**Status**: GEPLANT (Auf Eis gelegt zugunsten Funktions-Refactoring)  
**Entscheidung**: Funktions-Organisation im bestehenden Shader statt Multi-Pass  
**Nächster Schritt**: Shader-Code durch Funktionen und Kommentare organisieren
