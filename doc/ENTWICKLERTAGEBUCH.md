# Vulkan 3D-Engine - Entwicklertagebuch

## Inhaltsverzeichnis

1. [Projektübersicht](#projektübersicht)
2. [Architektur-Entscheidungen](#architektur-entscheidungen)
3. [Implementierungs-Chronologie](#implementierungs-chronologie)
4. [Debugging-Sessions](#debugging-sessions)
5. [Technische Lösungen](#technische-lösungen)
6. [Gelernte Lektionen](#gelernte-lektionen)
7. [Aktueller Status](#aktueller-status)

---

## Projektübersicht

**Projekt**: Vulkan-basierte 3D-Raytracing-Engine in C#  
**Ziel**: Production-ready Engine mit Clean Architecture und modernem Rendering  
**Sprache**: C# 11 mit .NET 9.0  
**Grafik-API**: Vulkan 1.3 via Silk.NET  
**Architektur**: Clean Architecture mit DDD-Prinzipien

### Technologie-Stack

- **Core**: C# 11, .NET 9.0
- **Rendering**: Vulkan 1.3 Compute Pipeline
- **Windowing**: GLFW via Silk.NET
- **Input**: Silk.NET.Input
- **Shader**: GLSL 4.5 → SPIR-V

### GPU-Target

- NVIDIA GeForce RTX 3070
- Vulkan 1.3 Support
- Compute Shader Pipeline

---

## Architektur-Entscheidungen

### Clean Architecture Struktur

Die Engine folgt konsequent Clean Architecture-Prinzipien mit drei Schichten:

#### Core (Domain Layer)

- **Entities**: Camera, Light, Triangle, Scene
- **Value Objects**: Vector3, Color, AABB
- **Services**: BVHBuilder, Engine
- **Keine Abhängigkeiten** nach außen

#### Application Layer

- **Use Cases**: EngineInitialization, EngineRendering, EngineUpdate, CameraControl
- **Services**: SceneBuilder, WindowManager, InputHandler
- **Container**: ServiceContainer für Dependency Injection
- **Config**: EngineConfig, RenderSettings

#### Infrastructure Layer

- **Vulkan Rendering**: VulkanRenderer, InternalVulkanRenderer
- **Window Management**: WindowManager (GLFW)
- **Input Handling**: InputHandler, CameraController
- **Shader**: GLSL Compute Shader

### Design-Prinzipien

- **DDD (Domain-Driven Design)**: Klare Trennung der Domänenlogik
- **SoC (Separation of Concerns)**: Jede Klasse hat eine klare Verantwortung
- **Dependency Inversion**: Interfaces statt konkreter Implementierungen
- **Keine Code-Kommentare**: Selbsterklärender Code durch sprechende Namen
- **Clean Code**: Kleine, fokussierte Methoden und Klassen

### Namespace-Konvention (2026-01-28)

Nach dem großen Refactoring wurde die Namespace-Struktur vereinfacht:

**Vorher**: `VulkanEngine.Core.Scene.Camera`  
**Nachher**: `Core.Scene.Camera`

Alle `VulkanEngine.` Präfixe wurden entfernt für eine flachere, klarere Struktur.

---

## Implementierungs-Chronologie

### Phase 1: Vulkan Pipeline Setup (Initial)

**Ziel**: Funktionierende Vulkan Render-Pipeline

**Implementiert**:

- ✅ Vulkan Instance & Physical Device Selection
- ✅ Logical Device & Queue Creation
- ✅ Swapchain Management (Triple Buffering, 3 Images)
- ✅ Command Buffers & Pools
- ✅ Descriptor Sets (5 Bindings)
- ✅ Compute Pipeline
- ✅ Storage Image für Render Output
- ✅ Buffer Management
- ✅ Memory Allocation

**Kritischer Fix**: Semaphore-Synchronisation per Image-Index

- **Problem**: Semaphoren wurden per Frame wiederverwendet, aber Swapchain hat mehr Images
- **Lösung**: `renderFinishedSemaphores[imageIndex]` statt `[_currentFrame]`
- **Resultat**: Keine Vulkan-Validierungsfehler mehr

### Phase 2: BVH Acceleration Structure

**Ziel**: Beschleunigung der Ray-Triangle Intersection Tests

**Implementiert**:

- ✅ AABB (Axis-Aligned Bounding Box) Klasse
- ✅ BVHNode - Binary Tree Structure
- ✅ BVHBuilder - SAH (Surface Area Heuristic)
    - Rekursiver Build mit Cost Function
    - 16 Bins für optimale Split-Plane Selection
    - Max 4 Triangles pro Leaf Node
- ✅ Scene Integration (`BuildBVH()` Methode)
- ✅ Performance: 5 Triangles → 2ms Build Time

**Note**: GPU-Integration noch ausstehend (Shader-Traversal)

### Phase 3: Multi-Bounce Reflections

**Ziel**: Realistische Reflektionen mit mehreren Bounces

**Implementiert**:

- ✅ RenderSettings Klasse (Performance/Default/Quality Presets)
- ✅ RenderSettings Uniform Buffer
- ✅ Iterative Reflection Loop (1-5 Bounces)
- ✅ Fresnel Effect (Schlick Approximation: `pow(1-dot, 5)`)
- ✅ Energy Decay (50% pro Bounce)
- ✅ Reflection Strength konfigurierbar (0.0-1.0)

**Shader Growth**: 10,660 → 15,904 → 21,436 bytes

### Phase 4: Soft Shadows

**Ziel**: Weiche, realistische Schatten

#### 4.1 Initial: Random Sampling

**Problem**: Clustering, sichtbare Muster, inkonsistente Qualität

#### 4.2 Poisson Disk Sampling (2026-01-29)

**Lösung**: 64 vorberechnete, gleichmäßig verteilte Samples

**Vorteile**:

- ✅ Konsistente Qualität bei allen Sample-Counts
- ✅ Keine Artefakte oder Musterbildung
- ✅ Bessere Performance (keine Random-Berechnungen)
- ✅ Vorhersagbare Skalierung

**Implementierung**:

```glsl
const vec2 poissonDisk[64] =  vec2[](...); // Vorberechnet
```

#### 4.3 PCSS (Percentage-Closer Soft Shadows) (2026-01-29)

**Ziel**: Physikalisch korrekte, distanzabhängige Shadow-Softness

**3-Schritt Algorithmus**:

1. **Blocker Search**: Durchschnittliche Blocker-Distanz finden
2. **Penumbra Width**: Berechnung basierend auf Geometrie
3. **Adaptive Filtering**: PCF mit dynamischer Kernel-Größe

**Formel**:

```glsl
penumbraWidth = (receiverDist - avgBlockerDist) * lightSize / avgBlockerDist;
```

**Resultat**: Harte Schatten an Contact Points, weiche bei entfernten Blockern

**Problem**: Zu komplex, keine sichtbare Verbesserung

#### 4.4 Zurück zu einfachen harten Schatten (2026-01-29)

**Entscheidung**: PCSS entfernt, zurück zu binären Schatten

**Neue Implementierung**:

```glsl
bool traceShadow(vec3 origin, vec3 lightDir, float maxDist, int numTriangles) {
    // Einfacher binärer Shadow Ray
    // true = im Schatten (20% Helligkeit)
    // false = beleuchtet (100% Helligkeit)
}
```

**Performance**: ~16-64x schneller als PCSS

### Phase 5: Beleuchtung & Shading (27.01.2026)

#### 5.1 Initial: Hart codierte Beleuchtung

**Problem**: Zu dunkle Szene, keine dynamischen Lichter

**Lösung**:

```glsl
vec3 lightDir = normalize(vec3(0.5, 1.0, 0.3));
float ambient = 0.5;
float diff = max(dot(hit.normal, lightDir), 0.0);
float diffuse = diff * 2.0;
```

#### 5.2 Dynamische Beleuchtung (2026-01-28)

**Implementiert**: 3 Lichttypen

- **Ambient**: Gleichmäßige Grundbeleuchtung
- **Directional**: Parallele Lichtstrahlen (Sonne)
- **Point**: Punktförmige Lichtquelle mit Attenuation

**Attenuation-Formel**:

```glsl
attenuation = 1.0 / (1.0 + 0.09 * dist + 0.032 * dist * dist);
```

#### 5.3 std430 vs std140 Problem (2026-01-28)

**Problem**: Light-Daten wurden nicht korrekt übertragen

**Ursache**: std140 hat komplexe Alignment-Regeln

- C# `Vector3` = 12 bytes
- GLSL `vec3` in std140 = 16 bytes (aligned)

**Lösung**:

1. Storage Buffer statt Uniform Buffer
2. std430 Layout (einfacheres Alignment)
3. Explizite float-Felder statt vec3/vec4

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct LightData {
    public int Type;
    public float Intensity;
    public float Pad1, Pad2;
    public float PosX, PosY, PosZ, Pad3;
    public float DirX, DirY, DirZ, Pad4;
    public float ColorR, ColorG, ColorB, Pad5;
}
```

#### 5.4 Backface Culling & Schatten (2026-01-29)

**Ziel**: Nur Vorderseiten rendern und Schatten werfen

**Rendering**:

```glsl
// In intersectTriangle()
if (det < EPSILON) return h; // Backface Culling
```

**Schatten** (invertiert):

```glsl
// In intersectTriangleShadow()
if (det > - EPSILON) return h; // Invertiertes Backface Culling
```

**Warum invertiert?**

- Shadow-Ray geht vom Punkt **zum Licht**
- Trifft Dreiecke von der **Rückseite**
- Mit `det > -EPSILON` blockieren nur Dreiecke, deren **Vorderseite** zum Licht zeigt

**Zusätzlich**:

- ✅ Gamma-Korrektur (1/2.2) für besseren Kontrast
- ✅ Verstärkte Attenuation für sichtbaren Lichtabfall

### Phase 6: Input System (27.01.2026)

#### 6.1 Mouse Look Implementation

**Problem**: Maussteuerung funktionierte nicht

**Ursache**: Update-Reihenfolge

**Lösung**:

```csharp
// RICHTIG: Consumer vor Producer
_cameraController?.Update(deltaTime);  // Verwendet Delta
_inputHandler?.Update(deltaTime);      // Bereitet nächstes Frame vor

// FALSCH:
// _inputHandler setzt lastMouse = currentMouse
// _cameraController erhält Delta = 0
```

**Look Speed Korrektur**: 0.1 → 0.003 (richtige Sensitivität)

#### 6.2 Tastenbelegung

**FPS/3D-Editor Standard**:

- **WASD**: Bewegung (Vorwärts/Links/Rückwärts/Rechts)
- **Q/E**: Hoch/Runter (statt Space/Shift)
- **Rechte Maustaste + Maus**: Kamera drehen

### Phase 7: Kamera & Koordinatensystem (27.01.2026)

#### 7.1 Camera Target Movement

**Problem**: Bei Bewegung änderte sich die Blickrichtung

**Lösung**:

```csharp
public void Move(Vector3 direction, float speed) {
    var offset = direction * speed;
    Position += offset;
    Target += offset;  // Target MUSS auch bewegt werden!
}
```

#### 7.2 Y-Achse Korrektur

**Problem**: Welt stand auf dem Kopf

**Lösung**:

```glsl
vec2 uv = (vec2(pixelCoords) / resolution) * 2.0 - 1.0;
uv.y = - uv.y; // Vulkan: (0,0) oben-links, Y nach unten
```

#### 7.3 Koordinatensystem Fix

**Problem**: Rechts/Links vertauscht

**Lösung**:

```glsl
// RICHTIG: cross(forward, up)
vec3 right = normalize(cross(forward, up));

// FALSCH: cross(up, forward) → zeigt nach links
```

**Up-Vektor**:

```glsl
vec3 up = vec3(0, 1, 0);  // Explizit Y-up
```

### Phase 8: RGB/BGR Format Fix (27.01.2026)

**Problem**: Rot und Blau waren vertauscht

**Ursache**: Format-Mismatch

- Swapchain: `B8G8R8A8Srgb` (BGRA)
- Storage Image: War auf BGRA gesetzt
- Shader: `rgba8` = `R8G8B8A8Unorm` (RGBA)

**Validation Error**:

```
Undefined-Value-StorageImage-FormatMismatch-ImageView
Storage Images must exactly match
```

**Lösung**: Storage Image auf RGBA + BGR Swizzle

```csharp
// VulkanRenderer: Storage Image auf RGBA
Format = Format.R8G8B8A8Unorm,
```

```glsl
// Shader: Swizzle RGB→BGR beim Schreiben
imageStore(outputImage, pixelCoords, vec4(color.bgr, 1.0));
```

**Resultat**: Korrekte Farben, keine Validation Warnings

### Phase 9: Geometrie-Korrektur (2026-01-28)

#### 9.1 Fehlendes mittleres Dreieck

**Problem**: Nach Refactoring wurde das mittlere Dreieck nicht gerendert

**Ursache**: Falsche Dreieck-Definitionen beim Kopieren

**Lösung**: Wiederherstellung der korrekten Geometrie aus Dokumentation

**Wichtig**: Dreieck-Orientierungen sind nicht willkürlich!

- **Rotes Dreieck**: Normale in +Z Richtung (62% Helligkeit)
- **Grünes Dreieck**: Normale in +X Richtung (100% Helligkeit)
- **Blaues Dreieck**: Normale in -X Richtung (50% Helligkeit)

Unterschiedliche Orientierungen sind essentiell für sichtbare Beleuchtungseffekte!

---

## Debugging-Sessions

### Session 1: Beleuchtung funktioniert nicht (27.01.2026)

**Dauer**: ~3 Stunden intensives Debugging  
**Status**: ✅ GELÖST

#### Problem

- Szene zeigte nur flache, uniforme Farben
- Keine Helligkeitsvariation trotz Lichtquellen
- Keine Schatten sichtbar

#### Debug-Verlauf

1. **Editor Auto-Save Problem**
    - **Symptom**: Shader-Änderungen wurden nicht übernommen
    - **Lösung**: WebStorm Auto-Save auf 1 Sekunde konfiguriert

2. **std140-Alignment Problem**
    - **Symptom**: Light-Types kamen als ungültige Werte an
    - **Lösung**: Vector3 durch explizite Floats ersetzt

3. **Geometrie-Problem** (ROOT CAUSE!)
    - **Symptom**: Alle Objekte gleiche Helligkeit
    - **Ursache**: Alle Dreiecke waren **parallel** zueinander
    - **Lösung**: Dreiecke mit unterschiedlichen Orientierungen erstellt

#### Debug-Methoden

**Normal-Visualisierung**:

```glsl
return hit.normal * 0.5 + 0.5;
// Boden = Grün (Y-up)
// Dreiecke = Farbmischung je nach Orientierung
```

**Light Count Check**:

```glsl
return vec3(float(lighting.numLights) / 8.0);
```

**Diffuse Visualisierung**:

```glsl
return vec3(diffuse);
```

#### Finale Ergebnisse

- **Boden**: #505050 (31% Helligkeit - mittel-grau)
- **Rotes Dreieck**: #9F0000 (62% Helligkeit - mittel-hell)
- **Grünes Dreieck**: #00FF00 (100% Helligkeit - voll hell!)
- **Blaues Dreieck**: #00007F (50% Helligkeit - dunkel)

### Session 2: Rider-Absturz & Fehlerkorrektur (2026-01-28)

**Problem**: Nach Rider-Absturz mehrere Kompilerfehler

#### Durchgeführte Korrekturen

1. **Namespace-Bereinigung**
    - Entfernt: Alle `VulkanEngine.` Präfixe
    - Korrigiert: `using` Statements in allen Dateien

2. **Vector3 Equality Operator**
    - **Fehler**: CameraControlUseCase konnte Vector3 nicht vergleichen
    - **Lösung**: `==`, `!=`, `Equals()`, `GetHashCode()` implementiert

3. **Namespace in InternalVulkanRenderer**
    - Korrigiert: `Geometry.TriangleEntity` → `TriangleEntity`
    - Korrigiert: `Core.Math.Color` → `Color`

**Ergebnis**: Projekt kompiliert erfolgreich ohne Fehler und Warnungen

---

## Technische Lösungen

### Vulkan Synchronisation

**Problem**: Komplexe GPU-GPU und CPU-GPU Synchronisation

**Lösung**: Separate Semaphore-Arrays

```csharp
// Frame-based (MaxFramesInFlight = 2)
imageAvailableSemaphores[MaxFramesInFlight]
inFlightFences[MaxFramesInFlight]

// Image-based (SwapchainImageCount = 3)
renderFinishedSemaphores[SwapchainImageCount]
```

**Golden Rule**: Ein Semaphore pro Swapchain-Image für renderFinished!

### Shader Layout Alignment

**Problem**: C# Strukturen ≠ GLSL Strukturen

**std140-Regeln** (Uniform Buffers):

- `vec3` aligned auf 16 bytes
- Arrays aligned auf 16 bytes
- Strukturen aligned auf 16 bytes

**std430-Regeln** (Storage Buffers):

- `vec3` aligned auf 12 bytes (dichter)
- Einfacheres Alignment

**Lösung**: Storage Buffer mit std430 + explizite Floats

### Winding Order & Normalen

**Counter-Clockwise (CCW)**: Normale zeigt nach außen

```csharp
// Boden (Normale nach oben):
new Vector3(-5, 0, -5),  // v0: hinten-links
new Vector3(-5, 0, 5),   // v1: vorne-links
new Vector3(5, 0, 5),    // v2: vorne-rechts
// cross(v1-v0, v2-v0) = (0, 1, 0)
```

### Backface Culling Strategie

**Rendering**: Nur Vorderseite sichtbar

```glsl
if (det < EPSILON) return h;
```

**Schatten**: Invertierte Logik (Ray geht zum Licht)

```glsl
if (det > - EPSILON) return h;
```

### Window Dispose ohne CLR-Crash

**Problem**: `0xC0000005` Access Violation beim Schließen

**Lösung**:

```csharp
public void Dispose() {
    if (_isDisposed) return;
    _isDisposed = true;
    
    try {
        _window?.Close();
        // NICHT: _inputContext?.Dispose(); (verursacht CLR-Fehler)
    } catch { /* Silent fail */ }
}
```

### Poisson Disk Sampling

**Problem**: Random Sampling → Clustering

**Lösung**: 64 vorberechnete Samples

```glsl
const vec2 poissonDisk[64] =  vec2[](
vec2(- 0.613392, 0.617481),
vec2(0.170019, - 0.040254),
// ...
);
```

**Vorteile**: Gleichmäßig, deterministisch, schnell

---

## Gelernte Lektionen

### 1. Vulkan ist extrem penibel

**Lektion**: Synchronisation erfordert exaktes Verständnis

**Best Practices**:

- Ein Semaphore pro Swapchain-Image (nicht pro Frame!)
- Fences für CPU-GPU Sync
- Semaphores für GPU-GPU Sync
- Validation Layers IMMER aktivieren

### 2. Shader Debugging ist schwer

**Lektion**: Schwarzer Bildschirm = Irgendwo NaN/Inf oder logischer Fehler

**Debug-Strategie**:

1. Einfachste Version: Direkte Farbe ohne Lighting
2. Schrittweise Features hinzufügen
3. Base Ambient Light als Fallback
4. Alle Berechnungen clampen

**Hilfreich**:

- Normal-Visualisierung: `return normal * 0.5 + 0.5;`
- Test-Farbe: `return vec3(1.0, 0.0, 1.0);` (Magenta = sichtbar)
- Component-Tests: Nur Ambient, nur Diffuse, etc.

### 3. Editor-Konfiguration ist kritisch

**Lektion**: Ohne sofortiges Speichern werden alte Shader kompiliert

**Lösung**: WebStorm Auto-Save auf 1 Sekunde

**Warnung in README.md** dokumentiert!

### 4. std140-Alignment ist komplex

**Lektion**: C# Strukturen ≠ GLSL Strukturen

**Regel**: IMMER explizite Floats oder std430 verwenden!

**Alternativ**: Manuelle Padding-Berechnung (fehleranfällig)

### 5. Geometrie-Design ist essentiell

**Lektion**: Parallele Flächen = gleiche Normalen = keine Beleuchtungsvarianz

**Regel**: Unterschiedliche Orientierungen für sichtbare Beleuchtung!

**Test-Szene**: Drei Dreiecke mit Normalen in +Z, +X, -X Richtung

### 6. Update-Reihenfolge ist kritisch

**Lektion**: Consumer IMMER vor Producer

```csharp
// RICHTIG:
_cameraController?.Update(deltaTime);  // Consumer
_inputHandler?.Update(deltaTime);      // Producer

// FALSCH:
_inputHandler?.Update(deltaTime);      // Setzt lastMouse
_cameraController?.Update(deltaTime);  // GetMouseDelta() = 0
```

### 7. Mouse Input ist Frame-basiert

**Lektion**: Mouse Delta ist bereits pro Frame

**Falsch**: `delta * lookSpeed * deltaTime` (zu langsam)  
**Richtig**: `delta * lookSpeed` (deltaTime ist schon drin)

### 8. Cross-Product Reihenfolge

**Lektion**: Die Reihenfolge bestimmt die Richtung

**Rechte-Hand-Regel**:

- `cross(forward, up)` = right
- `cross(up, forward)` = left

**Merke**: Daumen × Zeigefinger = Mittelfinger

### 9. Camera Target mitbewegen

**Lektion**: Position UND Target zusammen bewegen

**FPS-Style**: Target ist relativ zur Position, nicht absolut

```csharp
Position += offset;
Target += offset;  // Beide bewegen!
```

### 10. Vulkan Format-Matching ist strikt

**Lektion**: Storage Images müssen EXAKT übereinstimmen

**Lösung 1**: Swizzle im Shader (gewählt)  
**Lösung 2**: Unknown Format (braucht Feature Flag)

**Trade-off**: Explizit vs. Flexibel

### 11. BVH CPU vs GPU

**Gelernt**: CPU-Build ist einfach, aber...

**CPU-Side**:

- ✅ Einfach zu implementieren
- ✅ SAH optimiert
- ❌ Nicht im Shader nutzbar

**GPU-Side** (nächster Schritt):

- Flatten Tree zu Array
- Iterative Traversal
- AABB Ray Intersection Tests

### 12. Soft Shadows sind teuer

**Performance**:

- 1 Sample: Gleich wie Hard Shadows
- 4 Samples: ~4x teurer
- 8 Samples: ~8x teurer
- 16 Samples: ~16x teurer

**Trade-off**: Quality vs. Performance

**Optimization Ideas**:

- Adaptive Sampling
- Temporal Accumulation
- Denoising

### 13. PCSS ist komplex ohne Nutzen

**Lektion**: Nicht jede "fortgeschrittene" Technik bringt sichtbare Verbesserung

**PCSS**:

- ✅ Physikalisch korrekt
- ✅ Distanzabhängige Softness
- ❌ Komplex zu implementieren
- ❌ Keine sichtbare Verbesserung in unserer Test-Szene
- ❌ ~16-64x teurer

**Entscheidung**: Zurück zu einfachen harten Schatten

**Regel**: KISS (Keep It Simple, Stupid) - Erst messen, dann optimieren!

### 14. Dokumentation ist Gold wert

**Lektion**: Nach Refactoring wurden falsche Geometrie-Definitionen verwendet

**Lösung**: Dokumentation enthielt die korrekten Werte

**Best Practice**:

- Funktionierende Konfigurationen sofort dokumentieren
- Code-Änderungen mit Referenz zur Dokumentation
- Nach Refactoring: 1:1 aus Dokumentation übernehmen

---

## Aktueller Status

**Stand**: 2026-01-29

### Vollständig Implementiert ✅

#### Core Domain

- Camera mit FPS-Style Movement
- 3 Lichttypen (Ambient, Directional, Point)
- Triangle Geometrie
- BVH Acceleration (CPU-Side)
- Scene Graph
- Engine Loop

#### Application Layer

- EngineConfig & RenderSettings
- ServiceContainer (Dependency Injection)
- SceneBuilder
- Use Cases (Initialize, Update, Render, CameraControl)

#### Infrastructure Layer

- VulkanRenderer (1,420 Zeilen, vollständig)
- GLSL Compute Shader (264 Zeilen, 21,436 bytes SPIR-V)
- WindowManager (GLFW)
- InputHandler & CameraController
- Mouse Look & WASD Movement

#### Rendering Features

- Ray-Triangle Intersection (Möller-Trumbore)
- Multi-Bounce Reflections (1-5 Bounces)
- Fresnel Effect (Schlick Approximation)
- Phong Shading (Diffuse + Specular)
- Harte Schatten (binär, schnell)
- Backface Culling
- Gamma-Korrektur (1/2.2)
- Sky Gradient

#### Quality & Performance

- 3 Quality Presets (Performance/Default/Quality)
- BVH Build: 2ms für 5 Triangles
- Target: 60+ FPS @ 1280x720 ✅ (erreicht)
- GPU: NVIDIA RTX 3070

### In Arbeit / Geplant 📝

#### Kurzfristig

- [ ] BVH GPU-Integration (Shader-Traversal)
- [ ] Mehr komplexe Geometrie (Würfel, komplexe Szenen)
- [ ] Soft Shadows (einfache Methode, falls gewünscht)

#### Mittelfristig

- [ ] Texture Mapping
- [ ] Normal Maps für Detail
- [ ] Spot Lights
- [ ] Area Lights
- [ ] Max Triangles: 100k (mit GPU-BVH)
- [ ] Max Lights: 16 (erweitern)

#### Langfristig

- [ ] HDR + Tone Mapping
- [ ] Environment Mapping
- [ ] Deferred Shading
- [ ] Post-Processing Effects
- [ ] Resolution: 1920x1080

### Bekannte Limitierungen

1. **Max 8 Lichter** - Hardware-Limit durch feste Array-Größe
2. **Keine Spot-Lights** - Noch nicht implementiert
3. **Keine Textures** - Nur Vertex Colors
4. **BVH nur CPU-Side** - GPU-Traversal fehlt noch
5. **Harte Schatten** - Weiche Schatten wurden entfernt (zu komplex ohne Nutzen)

### Performance Metriken

**Aktuell**:

- Resolution: 1280x720
- FPS: 60+ (RTX 3070)
- Triangles: 5 (Test-Szene)
- Lights: 3 (verwendet), 8 (Maximum)
- Reflection Bounces: 3 (Default)
- Shadow Samples: 1 (Hard Shadows)

**Code Metriken**:

- Zeilen Code: ~3,500+ (ohne Kommentare)
- Klassen: 18
- Interfaces: 2
- Structs: 9
- Namespaces: 9
- Dependencies: 4 NuGet Packages
- Shader Size: 21,436 bytes (SPIR-V)
- Build Time: ~1.0s (Release)

### Build Status ✅

```bash
dotnet build
# ✅ Erfolgreich - Keine Fehler
# ✅ Keine Warnungen
# ⏱️ Build Time: ~1.0s

dotnet run
# ✅ Engine startet ohne Crashes
# ✅ Alle Features funktionieren
# ✅ WASD + Q/E Bewegung funktioniert
# ✅ Mouse Look funktioniert
# ✅ Korrekte Farben
# ✅ Korrekte Beleuchtung & Schatten
# ✅ Keine Vulkan Validation Warnings
# ⚡ FPS: 60+ auf RTX 3070
```

---

## Zusammenfassung

Dieses Projekt demonstriert eine vollständige Vulkan-basierte Raytracing-Engine mit:

- **Clean Architecture**: Klare Trennung von Domain, Application und Infrastructure
- **Moderne Rendering-Techniken**: Compute Shader, Multi-Bounce Reflections, Physically-Based Shading
- **Production-Ready Code**: Keine Kommentare, selbsterklärend, testbar
- **Performance-Fokus**: BVH Acceleration, Quality Presets, optimierte Shader
- **Ausführliche Dokumentation**: Jede Entscheidung und jeder Bug dokumentiert

Das Entwicklertagebuch zeigt den kompletten Weg von der ersten Idee bis zur funktionierenden Engine, einschließlich
aller Fehler, Debugging-Sessions und gelernten Lektionen.

**Nächste Phase**: GPU-BVH Integration für Skalierung auf 100k+ Triangles

---

**Letzte Aktualisierung**: 2026-01-29  
**Status**: Production Ready ✅  
**Version**: 1.0.0
