# Dynamische Beleuchtung

## 2026-01-28

## Status: ✅ Vollständig Implementiert und Funktionsfähig

Die dynamische Beleuchtung ist vollständig implementiert und funktioniert korrekt mit allen drei Lichttypen.

## Wichtige Korrektur: std430 statt std140

### Problem

Die Lichtdaten wurden nicht korrekt an den Shader übertragen. Alle Objekte waren schwarz oder hatten falsche
Beleuchtung.

### Ursache

Das `std140` Layout in GLSL hat komplexe Alignment-Regeln, die nicht 1:1 mit C#-Strukturen übereinstimmen. Insbesondere
werden `vec3` auf 16 Bytes aligned.

### Lösung

1. **Storage Buffer statt Uniform Buffer** für Lichtdaten
2. **std430 Layout** statt std140 (einfacheres Alignment)
3. **Explizite float-Felder** statt vec3/vec4 in der Light-Struktur

### Geänderte Dateien

- `Infrastructure/Rendering/Vulkan/Shaders/raytracing.comp` - std430 + explizite floats
- `Infrastructure/Vulkan/InternalVulkanRenderer.cs` - StorageBuffer statt UniformBuffer

## Lichtquellen-System

### Unterstützte Lichttypen

| Type | Name        | Beschreibung                    | Parameter                   |
|------|-------------|---------------------------------|-----------------------------|
| 0    | Ambient     | Gleichmäßige Grundbeleuchtung   | intensity, color            |
| 1    | Directional | Parallele Lichtstrahlen (Sonne) | direction, intensity, color |
| 2    | Point       | Punktförmige Lichtquelle        | position, intensity, color  |

### Aktuelle Szenen-Konfiguration

```csharp
scene.AddLight(LightEntity.CreateAmbient(Color.White, 0.05f));
scene.AddLight(LightEntity.CreateDirectional(new Vector3(0.5f, -1.0f, 0.3f), Color.White, 0.8f));
scene.AddLight(LightEntity.CreatePoint(new Vector3(-3, 4, 2), new Color(1.0f, 0.9f, 0.8f), 0.5f));
```

### Empfohlene Intensitätswerte

| Lichttyp    | Min | Standard | Max | Bemerkung                      |
|-------------|-----|----------|-----|--------------------------------|
| Ambient     | 0.0 | 0.05     | 0.2 | Höhere Werte → flache Schatten |
| Directional | 0.3 | 0.8      | 1.2 | Hauptlichtquelle               |
| Point       | 0.2 | 0.5      | 1.0 | Mit Attenuation                |

## Shader-Implementierung

### Light-Struktur (std430, 64 bytes)

```glsl
struct Light {
    int type;           // 0=Ambient, 1=Directional, 2=Point
    float intensity;
    float pad1;
    float pad2;
    float posX;
    float posY;
    float posZ;
    float pad3;
    float dirX;
    float dirY;
    float dirZ;
    float pad4;
    float colorR;
    float colorG;
    float colorB;
    float pad5;
};

layout(std430, binding = 3) buffer LightUBO {
    int numLights;
    int _pad1;
    int _pad2;
    int _pad3;
    Light lights[8];
} lighting;
```

### Beleuchtungsberechnung

```glsl
// Ambient Light
result += color * lightColor * light.intensity;

// Directional Light (mit Schatten + Specular)
vec3 lDir = normalize(-lightDir);
float diff = max(dot(normal, lDir), 0.0);
float shadow = traceShadow(...);
result += color * lightColor * diff * light.intensity * shadow;
result += lightColor * spec * 0.3 * light.intensity * shadow;

// Point Light (mit Attenuation + Schatten + Specular)
float attenuation = 1.0 / (1.0 + 0.09 * dist + 0.032 * dist * dist);
result += color * lightColor * diff * light.intensity * attenuation * shadow;
```

## C#-Strukturen

### LightData (64 bytes, Sequential Layout)

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct LightData
{
    public int Type;
    public float Intensity;
    public float Pad1, Pad2;
    public float PositionX, PositionY, PositionZ, Pad3;
    public float DirectionX, DirectionY, DirectionZ, Pad4;
    public float ColorR, ColorG, ColorB, Pad5;
}
```

### LightUniformData (528 bytes)

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct LightUniformData
{
    public int NumLights;
    public int Pad1, Pad2, Pad3;  // 16 bytes header
    public LightData Light0;      // 8 x 64 bytes
    public LightData Light1;
    // ... Light2-Light7
}
```

## Features

### Beleuchtung

- ✅ Ambient Light (gleichmäßige Grundhelligkeit)
- ✅ Directional Light (paralleles Licht mit Richtung)
- ✅ Point Light (Punktlicht mit Attenuation)
- ✅ Bis zu 8 Lichter pro Szene
- ✅ Blinn-Phong Specular Highlights

### Schatten

- ✅ Hard Shadows (1 Sample)
- ✅ Soft Shadows (Monte Carlo, 1-16 Samples)
- ✅ Shadow Factor: 0.3 (Schatten) bis 1.0 (beleuchtet)

### Attenuation (Point Light)

```
attenuation = 1.0 / (1.0 + 0.09 * distance + 0.032 * distance²)
```

## Debugging-Tipps

### Light-Types prüfen

```glsl
return vec3(float(lighting.lights[0].type) / 2.0, ...);
```

### Intensitäten prüfen

```glsl
return vec3(light0.intensity, light1.intensity, light2.intensity);
```

### Normalen prüfen

```glsl
return normal * 0.5 + 0.5;
```

## Bekannte Einschränkungen

1. **Max 8 Lichter** - Hardware-Limit durch feste Array-Größe
2. **Keine Spot-Lights** - Noch nicht implementiert
3. **Keine Licht-Schatten-Maps** - Nur Ray-traced Shadows

## Geänderte Dateien

| Datei                                                     | Änderung                                                      |
|-----------------------------------------------------------|---------------------------------------------------------------|
| `Core/Scene/Light/LightEntity.cs`                         | LightType Enum korrigiert (0=Ambient, 1=Directional, 2=Point) |
| `Infrastructure/Rendering/Vulkan/Shaders/raytracing.comp` | std430 + explizite floats                                     |
| `Infrastructure/Vulkan/InternalVulkanRenderer.cs`         | StorageBuffer, neue LightUniformData                          |
| `Application/Scene/SceneBuilderService.cs`                | Optimierte Lichtintensitäten                                  |

### Szenen-Konfiguration (SceneBuilderService.cs)

```csharp
scene.AddLight(LightEntity.CreateAmbient(Color.White, 0.2f));
scene.AddLight(LightEntity.CreateDirectional(new Vector3(0.5f, -1.0f, 0.3f), Color.White, 1.5f));
scene.AddLight(LightEntity.CreatePoint(new Vector3(-3, 4, 2), new Color(1.0f, 0.9f, 0.8f), 2.0f));
```

## Shader-Implementierung (raytracing.comp)

### Light-Daten-Struktur (std140 aligned)

```glsl
struct Light {
    int type;           // 0=Ambient, 1=Directional, 2=Point
    float intensity;
    vec2 _pad;
    vec3 position;
    float _pad2;
    vec3 direction;
    float _pad3;
    vec3 color;
    float _pad4;
};
```

### Beleuchtungs-Funktion

```glsl
vec3 shade(Hit hit, vec3 rayDir, int numTriangles) {
    vec3 color = hit.color;
    vec3 result = vec3(0.0);

    for (int i = 0; i < lighting.numLights; i++) {
        Light light = lighting.lights[i];

        if (light.type == 0) {  // Ambient
                                result += color * light.color * light.intensity;
        }
        else if (light.type == 1) {  // Directional
                                     // Diffuse + Specular + Shadows
        }
        else if (light.type == 2) {  // Point
                                     // Diffuse + Specular + Shadows + Attenuation
        }
    }

    return clamp(result, 0.0, 1.0);
}
```

## C# Daten-Transfer (InternalVulkanRenderer.cs)

### LightData-Struktur (64 bytes, std140 aligned)

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct LightData
{
    public int Type;
    public float Intensity;
    public float Pad1;
    public float Pad2;
    
    public float PositionX;
    public float PositionY;
    public float PositionZ;
    public float Pad3;
    
    public float DirectionX;
    public float DirectionY;
    public float DirectionZ;
    public float Pad4;
    
    public float ColorR;
    public float ColorG;
    public float ColorB;
    public float Pad5;
}
```

### Daten-Transfer

```csharp
var lights = scene.Lights.Take(8).ToArray();
for (int i = 0; i < lights.Length; i++)
{
    var lightEntry = new LightData
    {
        Type = (int)lights[i].Type,
        Intensity = lights[i].Intensity,
        PositionX = lights[i].Position.X,
        // ... etc
    };
    lightData.SetLight(i, lightEntry);
}
```

## Features

### Beleuchtung

- ✅ Ambient Light für Grundhelligkeit
- ✅ Directional Light mit Schatten
- ✅ Point Light mit Attenuation
- ✅ Bis zu 8 Lichter pro Szene
- ✅ Blinn-Phong Specular Highlights

### Schatten

- ✅ Hard Shadows (1 Sample)
- ✅ Soft Shadows (Monte Carlo, 1-16 Samples)
- ✅ Konfigurierbare Shadow Softness
- ✅ Shadow Factor: 0.3 (Schatten) bis 1.0 (beleuchtet)

### Performance

- ✅ std140-Layout für GPU-Kompatibilität
- ✅ Konfigurierbare Qualitätseinstellungen
- ✅ Shadow Samples anpassbar (Performance vs. Quality)

## Dateien

- `Infrastructure/Rendering/Vulkan/Shaders/raytracing.comp` - Shader mit dynamischer Beleuchtung
- `Infrastructure/Vulkan/InternalVulkanRenderer.cs` - Light-Daten-Transfer
- `Application/Scene/SceneBuilderService.cs` - Lichtquellen-Definition
- `Core/Scene/Light/LightEntity.cs` - Light-Entity mit Factory-Methoden
