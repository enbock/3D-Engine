# Vulkan Raytracing Engine - Native C#

Eine native 3D Raytracing-Engine in C# mit Vulkan, entwickelt mit Silk.NET.

## 📚 Dokumentation

Die vollständige Projektdokumentation findest du im Verzeichnis **[doc/](./doc/)**:

- **[Entwicklertagebuch](./doc/ENTWICKLERTAGEBUCH.md)** - Vollständige Implementierungs-Historie, Debugging-Sessions und
  gelernte Lektionen
- **[Dokumentations-Übersicht](./doc/README.md)** - Übersicht über alle Dokumente

Für technische Details siehe auch **[IMPLEMENTATION_STATUS.md](./IMPLEMENTATION_STATUS.md)** - Aktueller Stand der
Implementierung.

---

## ⚠️ WICHTIG: Entwicklungsumgebung

### Editor Auto-Save Konfiguration

**KRITISCH für Shader-Entwicklung**: WebStorm/Rider muss sofort speichern!

**Problem**:

- Standardmäßig verzögertes Speichern (mehrere Sekunden idle time)
- Shader-Kompilierung verwendet alte Dateiversion
- Debugging zeigt falsche/inkonsistente Ergebnisse

**Lösung**:

1. WebStorm: `Settings → Appearance & Behavior → System Settings → Synchronization`
2. Setze "Save files automatically if application is idle for" auf **1 Sekunde**
3. Oder: Aktiviere "Save files on frame deactivation"

**Test**: Nach Shader-Änderung 1-2 Sekunden warten, dann kompilieren.

## 🎯 Features

- **Vulkan API** - Native High-Performance Rendering
- **Compute Shader Raytracing** - GPU-beschleunigtes Raytracing
- **BVH Acceleration** - Surface Area Heuristic für schnelle Ray-Intersection
- **Multi-Bounce Reflections** - Konfigurierbare Reflection Bounces (1-5)
- **Soft Shadows** - Monte Carlo Shadow Sampling (1-16 Samples)
- **Clean Architecture** - DDD-Prinzipien mit Core/Application/Infrastructure
- **Realtime Camera Control** - WASD + Mouse Look
- **Dynamic Lighting** - Directional, Point und Ambient Lights
- **Fresnel Effect** - Physikalisch korrekte Reflections
- **Configurable Quality** - Performance/Default/Quality Presets

## 🛠️ Technologie Stack

- **C# .NET 9** - Moderne async/await Syntax
- **Silk.NET** - .NET Bindings für Vulkan, GLFW, Input
- **Vulkan SDK** - Low-Level Graphics API
- **GLSL Compute Shaders** - GPU Raytracing

## 📋 Voraussetzungen

- .NET 9 SDK
- Vulkan SDK (von https://vulkan.lunarg.com/)
- Vulkan-fähige GPU

## 🚀 Installation & Start

```bash
# Projekt builden
dotnet build

# Engine starten
dotnet run
```

## 🎮 Steuerung

### Kamera-Bewegung:

- **W** - Vorwärts
- **S** - Rückwärts
- **A** - Links
- **D** - Rechts
- **Q** - Hoch
- **E** - Runter

### Kamera-Rotation:

- **Rechte Maustaste + Bewegen** - Kamera drehen (Look Speed: 0.003)
- **Maus X-Bewegung** - Yaw (Links/Rechts)
- **Maus Y-Bewegung** - Pitch (Hoch/Runter)

### Sonstiges:

- **ESC** - Beenden

## 📁 Projektstruktur

```
VulkanEngine/
├── Core/
│   ├── Entities/
│   │   ├── Camera.cs           # Kamera mit View/Projection
│   │   ├── Light.cs            # Light System (3 Types)
│   │   └── Triangle.cs         # Basis-Geometrie
│   ├── Interfaces/
│   │   ├── IRenderer.cs        # Renderer Interface
│   │   └── IInputHandler.cs    # Input Interface
│   ├── Math/
│   │   ├── Vector3.cs          # 3D Vektor Mathematik
│   │   ├── Color.cs            # Farbverwaltung
│   │   └── AABB.cs             # Axis-Aligned Bounding Box
│   ├── Acceleration/
│   │   ├── BVHNode.cs          # BVH Tree Node
│   │   └── BVHBuilder.cs       # SAH-basierter BVH Builder
│   ├── Services/
│   │   └── Engine.cs           # Main Engine Loop
│   └── Scene.cs                # Szenen-Graph mit BVH
├── Application/
│   ├── Container/
│   │   └── ServiceContainer.cs # Dependency Injection
│   ├── Services/
│   │   └── SceneBuilder.cs     # Demo-Szene Builder
│   └── EngineConfig.cs         # Engine Konfiguration
├── Infrastructure/
│   ├── Vulkan/
│   │   ├── VulkanRenderer.cs   # Vulkan Rendering
│   │   └── Shaders/
│   │       └── raytracing.comp # Raytracing Compute Shader
│   ├── Window/
│   │   └── WindowManager.cs    # GLFW Window Management
│   └── Input/
│       ├── InputHandler.cs     # Keyboard & Mouse
│       └── CameraController.cs # FPS-Style Camera
└── Program.cs                  # Entry Point
```

## 🏗️ Architektur-Prinzipien

### Clean Architecture Layers:

1. **Core** - Domain Logic (Entities, Interfaces, Services)
2. **Application** - Use Cases (SceneBuilder, Container, Config)
3. **Infrastructure** - External Dependencies (Vulkan, Window, Input)

### Design Patterns:

- **Dependency Injection** - ServiceContainer
- **Strategy Pattern** - IRenderer Interface
- **Observer Pattern** - Event-basiertes Window Management
- **Builder Pattern** - SceneBuilder

### Code-Qualität:

- ✅ Keine Kommentare im Code
- ✅ Single Responsibility Principle
- ✅ Separation of Concerns
- ✅ Inverse Dependencies
- ✅ Modern async/await syntax
- ✅ Type-safe mit C# Generics

## 🎨 Raytracing Features

### Acceleration:

- **BVH (Bounding Volume Hierarchy)** - Binary tree acceleration structure
- **SAH (Surface Area Heuristic)** - Optimal split plane selection
- **Adaptive Leaf Size** - Max 4 triangles per leaf
- **Early Ray Termination** - Efficient traversal

### Geometrie:

- Triangle Mesh Support
- Analytische Intersection Tests
- AABB (Axis-Aligned Bounding Box) Culling

### Beleuchtung:

- **Directional Light** - Sonne/Mond-ähnlich
- **Point Light** - Punktlichtquelle mit Attenuation
- **Ambient Light** - Globale Beleuchtung

### Shading:

- Diffuse (Lambertian)
- Specular (Phong, Exponent 32)
- Soft Shadows (Monte Carlo Sampling, 1-16 Samples)
- Area Light Approximation (Disk Sampling)
- Multi-Bounce Reflections (1-5 Bounces, konfigurierbar)
- Fresnel Effect (Physikalisch korrekte Reflection Strength)
- Energy Decay (50% pro Bounce)

### Quality Presets:

- **Performance**: 1 Bounce, 1 Shadow Sample (Hard), 0% Softness
- **Default**: 3 Bounces, 4 Shadow Samples (Soft), 5% Softness
- **Quality**: 5 Bounces, 8 Shadow Samples (Very Soft), 10% Softness

### Optimierungen:

- Compute Shader basiert (16x16 Work Groups)
- GPU-seitige Szenen-Daten (SSBO)
- Uniform Buffers für Kamera & Lights
- Early Ray Termination

## 📊 Performance

- **Auflösung**: 1280x720 (konfigurierbar)
- **Target**: 60 FPS
- **VSync**: Aktiviert (deaktivierbar)

## 🔧 Konfiguration

[EngineConfig.cs](VulkanEngine/Application/EngineConfig.cs):

```csharp
var config = new EngineConfig
{
    Title = "Vulkan Raytracing Engine",
    Width = 1280,
    Height = 720,
    VSync = true,
    EnableValidation = true,
    MaxFramesInFlight = 2
};
```

## 🎓 Von WebGL gelernt

Diese Engine basiert auf Erkenntnissen aus dem WebGL-Experiment:

- Raytracing Algorithmus aus `web/src/Infrastructure/Rendering/RaytracingShaders.ts`
- Scene Management aus `web/src/Core/Scene.ts`
- Camera Control aus `web/src/Core/Camera.ts`

**Verbesserungen gegenüber WebGL:**

- Native Performance (kein JavaScript Overhead)
- Vulkan statt WebGL (mehr Kontrolle)
- Compute Shader (statt Fragment Shader Tricks)
- Besser skalierbar für komplexe Szenen

## 📝 Lizenz

MIT License

## 🚧 Roadmap

- [x] Vollständige Vulkan Pipeline Implementation
- [x] Korrekte Semaphore-Synchronisation (per Image-Index)
- [x] BVH (Bounding Volume Hierarchy) für Performance
- [x] Multi-Bounce Reflections (konfigurierbar 1-5 Bounces)
- [x] Soft Shadows (Monte Carlo Sampling, 1-16 Samples)
- [x] FPS-Style Kamera-Steuerung (WASD + Q/E + Mouse Look)
- [x] Korrekte Bildorientierung & Farb-Formate (keine Validation Warnings)
- [ ] Textures & Materials
- [ ] OBJ/GLTF Model Loading
- [ ] ImGui Debug UI
- [ ] Path Tracing Mode
- [ ] BVH GPU-Integration (Shader-basierte Traversal)
