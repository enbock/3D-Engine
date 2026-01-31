# Triangle Data Structure Alignment - Quick Reference

## Problem

Nach dem Hinzufügen von UV-Koordinaten und Texture-IDs zu `TriangleData` stimmen C# und GLSL-Strukturen nicht mehr
überein → Falsche Daten im Shader.

## Hauptproblem: std140 vs std430

**KRITISCHER FEHLER**: Die Shader verwendeten `std140` Layout für Storage Buffers, was FALSCH ist!

### Unterschied std140 vs std430

| Layout | Verwendung      | vec3 Alignment | vec3 Size |
|--------|-----------------|----------------|-----------|
| std140 | Uniform Buffers | 16 bytes       | 16 bytes  |
| std430 | Storage Buffers | 4 bytes        | 12 bytes  |

**std140**: `vec3` wird wie `vec4` behandelt (16-byte aligned, padding nach jedem vec3)
**std430**: `vec3` ist tatsächlich 12 bytes (kein unnötiges Padding)

## Lösung

### 1. Shader-Layout korrigieren (WICHTIGSTER FIX!)

**FALSCH** ❌:

```glsl
layout (std140, binding = 5) readonly buffer TriangleSSBO {
    Triangle triangles[];
};
```

**RICHTIG** ✅:

```glsl
layout (std430, binding = 5) readonly buffer TriangleSSBO {
    Triangle triangles[];
};
```

### 2. C# Struktur (`UniformDataStructures.cs`)

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct TriangleData
{
    public Vector3 V0;                  // Offset: 0 (12 bytes)
    public float Pad0;                  // Offset: 12 (4 bytes)
    public Vector3 V1;                  // Offset: 16 (12 bytes)
    public float Pad1;                  // Offset: 28 (4 bytes)
    public Vector3 V2;                  // Offset: 32 (12 bytes)
    public float Pad2;                  // Offset: 44 (4 bytes)
    public Vector3 Color;               // Offset: 48 (12 bytes)
    public float Transparency;          // Offset: 60 (4 bytes)
    public Vector3 N0;                  // Offset: 64 (12 bytes)
    public float IndexOfRefraction;     // Offset: 76 (4 bytes)
    public Vector3 N1;                  // Offset: 80 (12 bytes)
    public float Reflectivity;          // Offset: 92 (4 bytes)
    public Vector3 N2;                  // Offset: 96 (12 bytes)
    public float EnableSchlieren;       // Offset: 108 (4 bytes)
    public Vector2 UV0;                 // Offset: 112 (8 bytes)
    public Vector2 UV1;                 // Offset: 120 (8 bytes)
    public Vector2 UV2;                 // Offset: 128 (8 bytes)
    public int BaseColorTextureId;      // Offset: 136 (4 bytes)
    public int NormalTextureId;         // Offset: 140 (4 bytes)
}
// Total Size: 144 bytes (mit std430)
```

### 3. GLSL Struktur (alle *.comp Shader)

```glsl
struct Triangle {
    vec3 v0;                    // 12 bytes
    float pad0;                 // 4 bytes
    vec3 v1;                    // 12 bytes
    float pad1;                 // 4 bytes
    vec3 v2;                    // 12 bytes
    float pad2;                 // 4 bytes
    vec3 color;                 // 12 bytes
    float transparency;         // 4 bytes
    vec3 n0;                    // 12 bytes
    float ior;                  // 4 bytes
    vec3 n1;                    // 12 bytes
    float reflectivity;         // 4 bytes
    vec3 n2;                    // 12 bytes
    float enableSchlieren;      // 4 bytes
    vec2 uv0;                   // 8 bytes
    vec2 uv1;                   // 8 bytes
    vec2 uv2;                   // 8 bytes
    int baseColorTextureId;     // 4 bytes
    int normalTextureId;        // 4 bytes
};
// Total: 144 bytes
```

## Alignment-Regeln (std430)

| Typ   | Size | Alignment | Padding nach |
|-------|------|-----------|--------------|
| float | 4    | 4         | Nein         |
| vec2  | 8    | 8         | Nein         |
| vec3  | 12   | 4         | Nein*        |
| vec4  | 16   | 16        | Nein         |
| int   | 4    | 4         | Nein         |

*vec3 ist 4-byte aligned, nicht 16-byte wie bei std140!

## Betroffene Dateien

### C#

- `Infrastructure/Vulkan/Data/UniformDataStructures.cs`
- `Infrastructure/Vulkan/Helpers/VulkanBufferHelper.cs` (Datenübertragung)

### Shader (ALLE müssen std430 verwenden!)

- `Infrastructure/Rendering/Vulkan/Shaders/pass1_primary.comp`
- `Infrastructure/Rendering/Vulkan/Shaders/pass2_lighting.comp`
- `Infrastructure/Rendering/Vulkan/Shaders/pass2b_indirect.comp`
- `Infrastructure/Rendering/Vulkan/Shaders/pass3_reflections.comp`

## Workflow bei Struktur-Änderungen

1. ✅ C#-Struktur ändern (`UniformDataStructures.cs`)
2. ✅ **SICHERSTELLEN**: Storage Buffers verwenden `std430`
3. ✅ ALLE 4 Shader aktualisieren (identische Struktur!)
4. ✅ `compile_shaders.bat` ausführen
5. ✅ `dotnet build` ausführen
6. ✅ Testen

## Häufige Fehler

❌ **std140 für Storage Buffers** → vec3 wird als 16 bytes behandelt, alles durcheinander
❌ **Shader nicht aktualisiert** → Falsche Offsets, durcheinander Daten
❌ **Padding-Kalkulation für falsches Layout** → Bei std430 ist vec3 nur 12 bytes!

## Lesson Learned

> **Storage Buffers MÜSSEN std430 verwenden!**
> - std140 ist NUR für Uniform Buffers
> - std430 ist effizienter und hat realistischere Alignment-Regeln
> - Bei std430 ist vec3 tatsächlich 12 bytes, nicht 16!
