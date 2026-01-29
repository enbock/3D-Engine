# Multi-Pass Rendering - Schnellreferenz

## Toggle Multi-Pass / Single-Pass

In `Application/EngineConfig.cs`:

```csharp
// Multi-Pass Rendering (4 Passes, G-Buffer)
public bool UseMultiPassRendering { get; set; } = true;

// Single-Pass Rendering (Monolithischer Shader)
public bool UseMultiPassRendering { get; set; } = false;
```

## Architektur Übersicht

```
┌─────────────────┐
│  Pass 1: Primary │  → Ray Generation + Intersection
│  Rays            │     Output: G-Buffer (4 Images)
└────────┬─────────┘
         │
         ▼
┌─────────────────┐
│  Pass 2: Lighting│  → Diffuse + Specular + Shadows
│                  │     Output: Lit Color
└────────┬─────────┘
         │
         ▼
┌─────────────────┐
│  Pass 3: Reflect │  → Multi-Bounce Reflections
│  -ions           │     Output: Reflected Color
└────────┬─────────┘
         │
         ▼
┌─────────────────┐
│  Pass 4: Compo   │  → Gamma Correction + BGR
│  -site           │     Output: Final Image
└─────────────────┘
```

## Shader Dateien

| Shader                   | Zeilen | Beschreibung             |
|--------------------------|--------|--------------------------|
| `pass1_primary.comp`     | 147    | Ray Gen + Intersection   |
| `pass2_lighting.comp`    | 254    | Lighting + Shadows       |
| `pass3_reflections.comp` | 354    | Multi-Bounce Reflections |
| `pass4_composite.comp`   | 40     | Final Output             |

## G-Buffer

| Image          | Format  | Inhalt        | Alpha |
|----------------|---------|---------------|-------|
| gPosition      | RGBA32F | vec3 hitPoint | isHit |
| gNormal        | RGBA32F | vec3 normal   | -     |
| gAlbedo        | RGBA32F | vec3 color    | -     |
| gRayDir        | RGBA32F | vec3 rayDir   | -     |
| litColor       | RGBA32F | vec3 lit      | isHit |
| reflectedColor | RGBA32F | vec3 final    | 1.0   |

## Performance

| Mode        | VRAM     | Dispatches | Barriers |
|-------------|----------|------------|----------|
| Single-Pass | 8.3 MB   | 1          | 0        |
| Multi-Pass  | 307.1 MB | 4          | 3        |

**Trade-off**: 37x mehr VRAM für Modularität & Erweiterbarkeit

## Kompilierung

```bash
# Shader kompilieren
./compile_shaders.bat

# Projekt bauen
dotnet build

# Projekt starten
dotnet run
```

## Vorteile Multi-Pass

✅ **Debugging**: G-Buffer inspizierbar  
✅ **Modularität**: Jeder Pass unabhängig  
✅ **Erweiterbarkeit**: Neue Passes einfach  
✅ **Post-Processing**: Trivial hinzufügbar  
✅ **Temporal Effects**: TAA, Motion Blur möglich

## Nachteile Multi-Pass

❌ **VRAM**: 37x mehr Speicher  
❌ **Bandwidth**: Mehr Memory Reads/Writes  
❌ **Komplexität**: Mehr Code & Wartung

## Debugging

### G-Buffer Visualisierung (TODO)

```csharp
// In pass4_composite.comp
#define DEBUG_GBUFFER_POSITION
// Zeigt Position Buffer statt Final Image
```

### Pass-by-Pass Testing

```csharp
// In VulkanMultiPassTask.cs
public bool EnablePass2 { get; set; } = true;
public bool EnablePass3 { get; set; } = true;
// Passes deaktivierbar für Testing
```

## Zukünftige Features

Mit Multi-Pass möglich:

1. **Denoising** (Pass 4.5)
2. **TAA** (Temporal Accumulation)
3. **Bloom** (Post-Processing Pass)
4. **DOF** (Depth of Field)
5. **SSAO** (Screen-Space Ambient Occlusion)

## Dokumentation

- **[MULTI_PASS_IMPLEMENTATION_COMPLETE.md](./MULTI_PASS_IMPLEMENTATION_COMPLETE.md)** - Vollständige Doku
- **[SESSION_SUMMARY_MULTI_PASS.md](./SESSION_SUMMARY_MULTI_PASS.md)** - Implementierungs-Log
- **[ENTSCHEIDUNG_MULTI_PASS.md](./ENTSCHEIDUNG_MULTI_PASS.md)** - Ursprüngliche Entscheidung

---

**Status**: ✅ Produktionsbereit  
**Version**: 1.0  
**Datum**: 2026-01-29
