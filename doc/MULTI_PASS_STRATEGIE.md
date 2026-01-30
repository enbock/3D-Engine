# Multi-Pass Rendering Strategie

## Entscheidung

**Multi-Pass Rendering wurde als strategische Architektur gewählt**, da es langfristig besser für komplexe
Rendering-Features geeignet ist.

## Gründe für Multi-Pass

### 1. Modularität

Jeder Pass ist für ein spezifisches Feature verantwortlich:

- **Pass 1**: Primary Rays (G-Buffer Erstellung)
- **Pass 2**: Lighting (Shadows, Diffuse, Specular)
- **Pass 3**: Reflections (Multi-Bounce)
- **Pass 4**: Composite (Gamma, Tonemapping, Output)

### 2. Einfache Erweiterbarkeit

| Zukünftiges Feature | Implementierung                |
|---------------------|--------------------------------|
| Soft Shadows        | Pass 2 erweitern (Monte Carlo) |
| Caustics            | Neuer Pass vor Lighting        |
| Glass/Water         | Neuer Refraction Pass          |
| Transparencies      | OIT Pass nach Reflections      |
| Denoising           | Neuer Post-Process Pass        |
| Bloom/DOF           | Pass 4 erweitern               |

### 3. G-Buffer Vorteile

- Screen-Space Effekte möglich (SSR, SSAO)
- Temporal Accumulation für Denoising
- Post-Processing-Effekte trivial hinzufügbar

## Implementierte Optimierungen

### BVH-Integration in alle Passes ✅

- **Pass 1**: `traceBVH()` für Primary Rays
- **Pass 2**: `traceShadowBVH()` für Shadows
- **Pass 3**: `traceBVH()` + `traceShadowBVH()` für Reflections
- **Pass 4**: Kein Ray-Tracing (nur Compositing)

### Shared Memory ✅

- Camera-Daten werden in Shared Memory gecacht
- Reduziert redundante Berechnungen pro Workgroup

### Optimierte AABB-Intersection ✅

- `invDir` wird einmal pro Ray berechnet
- Nicht pro AABB-Test

## Architektur

```
┌─────────────────────────────────────────────────────────────┐
│                    Pass 1: Primary Rays                      │
│    Camera → traceBVH() → G-Buffer (Position, Normal, Albedo) │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                    Pass 2: Lighting                          │
│    G-Buffer → shade() + traceShadowBVH() → Lit Color        │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                    Pass 3: Reflections                       │
│    G-Buffer + Lit → traceBVH() + shade() → Final Color      │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                    Pass 4: Composite                         │
│    Final Color → Gamma Correction → Output Image            │
└─────────────────────────────────────────────────────────────┘
```

## Bindings pro Pass

### Pass 1 (7 Bindings)

| Binding | Typ           | Inhalt                                      |
|---------|---------------|---------------------------------------------|
| 0-3     | StorageImage  | G-Buffer (Position, Normal, Albedo, RayDir) |
| 4       | UniformBuffer | Camera                                      |
| 5       | StorageBuffer | Triangles                                   |
| 6       | StorageBuffer | BVH                                         |

### Pass 2 (10 Bindings)

| Binding | Typ           | Inhalt              |
|---------|---------------|---------------------|
| 0-4     | StorageImage  | G-Buffer + LitColor |
| 5       | StorageBuffer | Lights              |
| 6       | StorageBuffer | Triangles           |
| 7       | UniformBuffer | Settings            |
| 8       | UniformBuffer | Camera              |
| 9       | StorageBuffer | BVH                 |

### Pass 3 (11 Bindings)

| Binding | Typ           | Inhalt                               |
|---------|---------------|--------------------------------------|
| 0-5     | StorageImage  | G-Buffer + LitColor + ReflectedColor |
| 6       | StorageBuffer | Triangles                            |
| 7       | StorageBuffer | Lights                               |
| 8       | UniformBuffer | Settings                             |
| 9       | UniformBuffer | Camera                               |
| 10      | StorageBuffer | BVH                                  |

### Pass 4 (3 Bindings)

| Binding | Typ           | Inhalt                 |
|---------|---------------|------------------------|
| 0       | StorageImage  | ReflectedColor (Input) |
| 1       | StorageImage  | OutputImage            |
| 2       | UniformBuffer | Camera                 |

## Gelöschte Dateien

- `raytracing.comp` - Single-Pass Shader (entfernt)
- `raytracing.comp.spv` - Kompilierte Version (entfernt)
- `raytracing_bvh.comp` - BVH-Template (entfernt, in Multi-Pass integriert)

## Konfiguration

```csharp
// EngineConfig.cs
public static bool UseMultiPassRendering => true; // Jetzt permanent true
```
