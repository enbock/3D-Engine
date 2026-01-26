# Vulkan Raytracing Engine - Native C#

Eine native 3D Raytracing-Engine in C# mit Vulkan, entwickelt mit Silk.NET.

## 🎯 Features

- **Vulkan API** - Native High-Performance Rendering
- **Compute Shader Raytracing** - GPU-beschleunigtes Raytracing
- **Clean Architecture** - DDD-Prinzipien mit Core/Application/Infrastructure
- **Realtime Camera Control** - WASD + Mouse Look
- **Dynamic Lighting** - Directional, Point und Ambient Lights
- **Reflections** - Single-Bounce Reflektionen
- **Shadows** - Raytraced Hard Shadows

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
- **Space** - Hoch
- **Shift** - Runter

### Kamera-Rotation:
- **Rechte Maustaste + Bewegen** - Kamera drehen

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
│   │   └── Color.cs            # Farbverwaltung
│   ├── Services/
│   │   └── Engine.cs           # Main Engine Loop
│   └── Scene.cs                # Szenen-Graph
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

### Geometrie:
- Triangle Mesh Support
- Analytische Intersection Tests
- Bounding Volume Hierarchie (geplant)

### Beleuchtung:
- **Directional Light** - Sonne/Mond-ähnlich
- **Point Light** - Punktlichtquelle mit Attenuation
- **Ambient Light** - Globale Beleuchtung

### Shading:
- Diffuse (Lambertian)
- Specular (Phong, Exponent 32)
- Shadows (Hard Shadows via Shadow Rays)
- Reflections (Single-Bounce)

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
- [ ] Shader Compilation zur Runtime
- [ ] BVH (Bounding Volume Hierarchy) für Performance
- [ ] Multi-Bounce Reflections
- [ ] Soft Shadows
- [ ] Textures & Materials
- [ ] OBJ/GLTF Model Loading
- [ ] ImGui Debug UI
- [ ] Path Tracing Mode
