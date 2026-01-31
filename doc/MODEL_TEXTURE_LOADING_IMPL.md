# Model & Texture Loading Implementation

## Übersicht

Vollständige Implementierung für das Laden von 3D-Modellen (glTF/GLB) mit Textur-Unterstützung (BaseColor, Normal Maps).

## Neue Dateien

### Core Layer

- `Core/Assets/IModelLoader.cs` - Interface und Datenstrukturen für Model-Loading
- `Core/Scene/Geometry/TextureCoordinate.cs` - UV-Koordinaten Struct

### Infrastructure Layer

- `Infrastructure/Assets/ITextureLoader.cs` - Interface für Texture-Loader
- `Infrastructure/Assets/TextureHandle.cs` - Handle-Klasse für GPU-Texturen
- `Infrastructure/Assets/Texture.cs` - Vulkan-Textur Wrapper mit Dispose
- `Infrastructure/Assets/TextureLoader.cs` - Lädt Bilder und erstellt Vulkan-Texturen
- `Infrastructure/Assets/ModelLoader.cs` - Lädt glTF/GLB Modelle mit SharpGLTF

### Application Layer

- `Application/Assets/AssetService.cs` - Service für Asset-Management

## Verwendete Bibliotheken

- **SharpGLTF.Core** (1.0.2) - glTF/GLB Parser
- **StbImageSharp** (2.30.15) - Image Decoding (PNG, JPEG)

## Architektur

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  ┌─────────────────┐  ┌──────────────────────────────────┐ │
│  │  AssetService   │  │     ServiceContainer             │ │
│  └────────┬────────┘  └──────────────────────────────────┘ │
├───────────┼─────────────────────────────────────────────────┤
│           │              Core Layer                         │
│  ┌────────▼────────┐  ┌──────────────────────────────────┐ │
│  │  IModelLoader   │  │   SceneEntity / MeshData         │ │
│  └────────┬────────┘  └──────────────────────────────────┘ │
├───────────┼─────────────────────────────────────────────────┤
│           │           Infrastructure Layer                  │
│  ┌────────▼────────┐  ┌──────────────────────────────────┐ │
│  │   ModelLoader   │──│   TextureLoader                  │ │
│  └─────────────────┘  └─────────────┬────────────────────┘ │
│                                     │                       │
│  ┌──────────────────────────────────▼────────────────────┐ │
│  │              Vulkan GPU Resources                      │ │
│  │   VkImage, VkImageView, VkSampler, VkDeviceMemory     │ │
│  └───────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## API-Verwendung

### Modell laden

```csharp
// Im WorldUseCase
worldUseCase.SetModelLoader(modelLoader);
worldUseCase.LoadModel("my_model.glb");

// Oder Szene mit Modell initialisieren
worldUseCase.InitializeWithModel("scene.glb");
```

### Direkt über ModelLoader

```csharp
ModelLoader loader = serviceContainer.Resolve<ModelLoader>();
ModelData model = loader.LoadModel("assets/models/character.glb");
scene.AddModel(model);
```

### Mit SceneBuilderService

```csharp
SceneBuilderService.AddModelToScene(scene, modelLoader, "object.glb");
SceneBuilderService.CreateSceneWithModel(scene, modelLoader, "full_scene.glb");
```

## Datenstrukturen

### MaterialData

```csharp
public record MaterialData
{
    public Color BaseColor { get; set; }
    public TextureHandle? BaseColorTexture { get; set; }
    public TextureHandle? NormalTexture { get; set; }
    public float Metallic { get; set; }
    public float Roughness { get; set; }
    public float Transparency { get; set; }
    public float IndexOfRefraction { get; set; }
}
```

### TriangleData (GPU)

```csharp
struct TriangleData {
    vec3 V0, V1, V2;        // Vertices
    vec3 Color;              // BaseColor
    vec3 N0, N1, N2;        // Normals
    vec2 UV0, UV1, UV2;     // Texture-Koordinaten
    int BaseColorTextureId;  // -1 wenn keine Textur
    int NormalTextureId;     // -1 wenn keine Textur
}
```

## Unterstützte Features

### Modell-Formate

- ✅ glTF 2.0 (.gltf)
- ✅ GLB (.glb) - Binary glTF
- ✅ Embedded Texturen
- ✅ Externe Texturen

### Material-Eigenschaften

- ✅ BaseColor (Albedo)
- ✅ Normal Maps
- ✅ Metallic/Roughness Faktoren
- ✅ Transparenz (Alpha)

### Textur-Formate

- ✅ PNG
- ✅ JPEG
- ✅ Eingebettete Daten (data URI / embedded)

## Shader-Integration (TODO)

Die Triangle-Daten enthalten jetzt UV-Koordinaten und Texture-IDs.
Für das Rendering müssen die Shader erweitert werden:

1. Texture Array/Bindless Textures im DescriptorSet
2. UV-Interpolation im Raytracer
3. Texture Sampling mit BaseColorTextureId
4. Normal Mapping mit NormalTextureId

## Edge-Cases

### Fallback-Textur

Wenn eine Textur nicht geladen werden kann, wird eine 2x2 Magenta-Textur erstellt.

### Fehlende Normal Maps

Wenn keine Normal Map vorhanden, wird `NormalTextureId = -1` gesetzt. Der Shader sollte dann die Vertex-Normale
verwenden.

### Farbraum

- BaseColor: VK_FORMAT_R8G8B8A8_SRGB
- Normal Maps: VK_FORMAT_R8G8B8A8_UNORM (linear)

## Assets-Ordner

```
assets/
├── models/       # glTF/GLB Modelle
│   └── *.glb
└── textures/     # Standalone Texturen
    └── *.png
```

## Nächste Schritte

1. [ ] Shader-Unterstützung für Texturen
2. [ ] Mipmap-Generierung
3. [ ] Texture Caching (vermeiden doppelten Ladens)
4. [ ] Async Loading mit Placeholder-Textur
5. [ ] Vollständiges PBR (metallicRoughness Map, Occlusion, Emissive)
