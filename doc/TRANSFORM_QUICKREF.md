# Transform-System - Schnellreferenz

## Übersicht

Ein vollständiges Transform-System zum Positionieren, Rotieren und Skalieren von TriangleEntity-Objekten.

## Hauptkomponenten

### 1. Matrix4X4 (`Core.Math.Matrix4X4`)

4x4 Transformationsmatrix für 3D-Operationen.

**Statische Methoden:**

```csharp
Matrix4X4.Identity()
Matrix4X4.Translation(Vector3 position)
Matrix4X4.Scale(Vector3 scale)
Matrix4X4.RotationX(float angle)
Matrix4X4.RotationY(float angle)
Matrix4X4.RotationZ(float angle)
Matrix4X4.Rotation(Vector3 eulerAngles)
```

**Instanz-Methoden:**

```csharp
Vector3 TransformPoint(Vector3 point)
Vector3 TransformDirection(Vector3 direction)
```

### 2. TransformData (`Core.Math.TransformData`)

Datenklasse für Position, Rotation und Skalierung.

**Properties:**

```csharp
Vector3 Position
Vector3 Rotation  // Euler-Winkel in Radiant
Vector3 Scale
```

**Methoden:**

```csharp
Matrix4X4 GetMatrix()
void Translate(Vector3 offset)
void Rotate(Vector3 angles)
void ScaleBy(Vector3 factor)
void SetPosition(Vector3 position)
void SetRotation(Vector3 rotation)
void SetScale(Vector3 scale)
TransformData Clone()
```

### 3. TransformService (`Core.Scene.Transform.TransformService`)

Service zum Anwenden von Transformationen.

**Methoden:**

```csharp
TriangleEntity ApplyTransform(TriangleEntity triangle, TransformData transform)
List<TriangleEntity> ApplyTransform(List<TriangleEntity> triangles, TransformData transform)
void TransformInPlace(TriangleEntity triangle, TransformData transform)
void TransformInPlace(List<TriangleEntity> triangles, TransformData transform)
```

## Quick Start

### Basis-Beispiel

```csharp
using Core.Math;
using Core.Scene.Transform;

TransformService transformService = new();

TransformData transform = new(
    new Vector3(5, 2, 0),           // Position
    new Vector3(0, MathF.PI/4, 0),  // 45° um Y-Achse
    Vector3.One * 2                  // 2x vergrößert
);

TriangleEntity original = new(
    new Vector3(-1, 0, 0),
    new Vector3(1, 0, 0),
    new Vector3(0, 2, 0),
    Color.Red
);

TriangleEntity transformed = transformService.ApplyTransform(original, transform);
scene.AddTriangle(transformed);
```

### Mit GeometryGenerator

```csharp
GeometryGenerator.AddCubeWithTransform(
    scene,
    new TransformData(
        new Vector3(5, 0, 0),
        new Vector3(0, MathF.PI/4, 0),
        Vector3.One * 1.5f
    ),
    1.0f,
    Color.Red
);

GeometryGenerator.AddSphereWithTransform(
    scene,
    new TransformData(
        new Vector3(0, 2, 0),
        Vector3.Zero,
        new Vector3(1, 2, 1)  // Ellipsoid
    ),
    1.0f,
    12, 16,
    Color.Blue,
    ShadingMode.Smooth
);
```

### Inkrementelle Transformationen

```csharp
TransformData transform = new();
transform.SetPosition(new Vector3(0, 0, 0));

transform.Translate(new Vector3(5, 0, 0));  // Nach rechts
transform.Rotate(new Vector3(0, MathF.PI/6, 0));  // 30° um Y
transform.ScaleBy(new Vector3(2, 1, 1));  // 2x breiter

TriangleEntity result = transformService.ApplyTransform(triangle, transform);
```

### Batch-Transformation

```csharp
List<TriangleEntity> triangles = GetTriangles();
TransformData transform = new(Vector3.Zero, Vector3.Zero, Vector3.One * 2);

List<TriangleEntity> scaled = transformService.ApplyTransform(triangles, transform);

transformService.TransformInPlace(triangles, transform);
```

## Demo-Szene

```csharp
SceneBuilderService builder = new();
builder.CreateTransformDemoScene(scene);
```

## Wichtige Hinweise

1. **Matrix-Reihenfolge**: TRS (Translation * Rotation * Scale)
2. **Rotations-Reihenfolge**: Z-Y-X
3. **Winkel**: In Radiant (verwende `MathF.PI`)
4. **Normalen**: Werden automatisch korrekt transformiert
5. **Performance**: `ApplyTransform()` erstellt neue Objekte, `TransformInPlace()` modifiziert direkt

## Typische Anwendungsfälle

### Objekt platzieren

```csharp
transform.SetPosition(new Vector3(10, 0, 5));
```

### Um 90° drehen

```csharp
transform.SetRotation(new Vector3(0, MathF.PI/2, 0));
```

### Verdoppeln

```csharp
transform.SetScale(Vector3.One * 2);
```

### Ellipsoid erstellen

```csharp
TransformData ellipsoid = new(
    Vector3.Zero,
    Vector3.Zero,
    new Vector3(1, 2, 1)  // Höher als breit
);
```

### Animation vorbereiten

```csharp
for (int i = 0; i < 360; i += 10)
{
    float angle = i * MathF.PI / 180f;
    TransformData transform = new(
        Vector3.Zero,
        new Vector3(0, angle, 0),
        Vector3.One
    );
    TriangleEntity frame = transformService.ApplyTransform(baseTriangle, transform);
}
```

## Siehe auch

- [TRANSFORM_SYSTEM.md](TRANSFORM_SYSTEM.md) - Vollständige Dokumentation
- [ENTWICKLERTAGEBUCH.md](ENTWICKLERTAGEBUCH.md) - Implementierungs-Geschichte
