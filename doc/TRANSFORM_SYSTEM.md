# Transform-System für TriangleEntity

## Datum

2026-01-31

## Übersicht

Implementierung eines vollständigen Transform-Systems, das es ermöglicht, TriangleEntity-Objekte im 3D-Raum zu
transformieren (Position, Rotation, Skalierung).

## Implementierte Komponenten

### 1. Matrix4X4 (Core.Math)

**Datei:** `Core/Math/Matrix4X4.cs`

Eine 4x4-Matrix-Struktur für 3D-Transformationen:

**Features:**

- Identity-Matrix
- Translation (Verschiebung)
- Scale (Skalierung)
- Rotation um X-, Y-, und Z-Achse
- Kombinierte Rotation (Euler-Winkel)
- Matrix-Multiplikation
- TransformPoint (transformiert einen Punkt)
- TransformDirection (transformiert eine Richtung/Normale)

**Verwendung:**

```csharp
Matrix4X4 matrix = Matrix4X4.Translation(new Vector3(1, 2, 3));
Matrix4X4 rotation = Matrix4X4.RotationY(MathF.PI / 4);
Matrix4X4 scale = Matrix4X4.Scale(new Vector3(2, 2, 2));
Matrix4X4 combined = matrix * rotation * scale;

Vector3 transformedPoint = combined.TransformPoint(originalPoint);
Vector3 transformedNormal = combined.TransformDirection(originalNormal);
```

### 2. TransformData (Core.Math)

**Datei:** `Core/Math/Transform.cs`

Eine Klasse, die Position, Rotation und Skalierung kapselt:

**Properties:**

- `Position` - Vector3 für die Position im Raum
- `Rotation` - Vector3 für Euler-Winkel (in Radiant)
- `Scale` - Vector3 für Skalierungsfaktoren

**Methoden:**

- `GetMatrix()` - Erzeugt die kombinierte Transformationsmatrix (TRS - Translation, Rotation, Scale)
- `Translate(Vector3)` - Verschiebt das Objekt relativ
- `Rotate(Vector3)` - Rotiert das Objekt relativ
- `ScaleBy(Vector3)` - Skaliert das Objekt relativ
- `SetPosition(Vector3)` - Setzt absolute Position
- `SetRotation(Vector3)` - Setzt absolute Rotation
- `SetScale(Vector3)` - Setzt absolute Skalierung
- `Clone()` - Erstellt eine Kopie

**Verwendung:**

```csharp
TransformData transform = new TransformData(
    new Vector3(0, 1, 0),           // Position
    new Vector3(0, MathF.PI/4, 0),  // Rotation (45° um Y-Achse)
    Vector3.One * 2                  // Scale (2x vergrößert)
);

transform.Translate(new Vector3(1, 0, 0));
transform.Rotate(new Vector3(0, MathF.PI/6, 0));
```

### 3. TransformService (Core.Scene.Transform)

**Datei:** `Core/Scene/Transform/TransformService.cs`

Ein Service zur Anwendung von Transformationen auf TriangleEntity-Objekte:

**Methoden:**

- `ApplyTransform(TriangleEntity, TransformData)` - Erstellt ein neues transformiertes Triangle
- `ApplyTransform(List<TriangleEntity>, TransformData)` - Transformiert eine Liste von Triangles
- `TransformInPlace(TriangleEntity, TransformData)` - Transformiert ein Triangle direkt (in-place)
- `TransformInPlace(List<TriangleEntity>, TransformData)` - Transformiert eine Liste direkt

**Verwendung:**

```csharp
TransformService transformService = new();
TransformData transform = new(
    new Vector3(5, 0, 0),
    new Vector3(0, MathF.PI/2, 0),
    Vector3.One
);

TriangleEntity transformed = transformService.ApplyTransform(originalTriangle, transform);

transformService.TransformInPlace(existingTriangle, transform);
```

## Demo-Szene

Eine neue Methode `CreateTransformDemoScene` wurde zu `SceneBuilderService` hinzugefügt, die die
Transform-Funktionalität demonstriert:

```csharp
public void CreateTransformDemoScene(SceneEntity scene)
```

Diese Szene zeigt drei Versionen desselben Basis-Dreiecks mit unterschiedlichen Transformationen:

1. **Links** - Nur Translation (-3, 0, 0)
2. **Mitte** - Translation + Rotation (45° um Y) + Scale (1.5x)
3. **Rechts** - Translation + Rotation (30° um Z) + Scale (2x)

## Technische Details

### Matrix-Reihenfolge

Die Transformationen werden in der Reihenfolge TRS (Translation-Rotation-Scale) angewendet:

```csharp
Matrix = Translation * Rotation * Scale
```

### Rotations-Reihenfolge

Rotationen werden in der Reihenfolge Z-Y-X angewendet:

```csharp
Rotation = RotationZ * RotationY * RotationX
```

### Normalen-Transformation

Normalen werden als Richtungsvektoren transformiert (ohne Translation) und anschließend normalisiert:

```csharp
Vector3 transformedNormal = matrix.TransformDirection(normal).Normalized;
```

## Anwendungsfälle

### 1. Objekt positionieren

```csharp
TransformData transform = new();
transform.SetPosition(new Vector3(5, 2, 3));
TriangleEntity positioned = transformService.ApplyTransform(triangle, transform);
```

### 2. Objekt rotieren

```csharp
TransformData transform = new();
transform.SetRotation(new Vector3(0, MathF.PI / 4, 0)); // 45° um Y
TriangleEntity rotated = transformService.ApplyTransform(triangle, transform);
```

### 3. Objekt skalieren

```csharp
TransformData transform = new();
transform.SetScale(new Vector3(2, 1, 2)); // 2x in X und Z, 1x in Y
TriangleEntity scaled = transformService.ApplyTransform(triangle, transform);
```

### 4. Komplexe Transformation

```csharp
TransformData transform = new(
    new Vector3(10, 5, 0),          // Position
    new Vector3(0, MathF.PI/3, 0),  // 60° um Y
    new Vector3(1.5f, 1.5f, 1.5f)   // 1.5x vergrößert
);
TriangleEntity transformed = transformService.ApplyTransform(triangle, transform);
```

### 5. Mehrere Objekte transformieren

```csharp
List<TriangleEntity> triangles = GetTriangles();
TransformData transform = new(Vector3.Zero, Vector3.Zero, Vector3.One * 2);
List<TriangleEntity> scaledTriangles = transformService.ApplyTransform(triangles, transform);
```

## Architektur

Das Transform-System folgt den Clean-Code-Prinzipien:

- **Separation of Concerns**: Matrix-Mathematik (Matrix4X4), Transform-Daten (TransformData) und Transform-Logik (
  TransformService) sind getrennt
- **Single Responsibility**: Jede Klasse hat eine klare Verantwortung
- **Domain-Driven Design**: Alle Klassen befinden sich im Core-Layer als Teil der Domain-Logik
- **Immutability**: `ApplyTransform` erstellt neue Objekte, während `TransformInPlace` für Performance-kritische
  Szenarien vorhanden ist

## Performance-Hinweise

- Für einzelne Transformationen: Verwende `ApplyTransform()` (erstellt neue Objekte)
- Für Batch-Transformationen: Verwende `TransformInPlace()` (modifiziert direkt)
- Die Matrix-Berechnung erfolgt nur einmal pro Transform-Operation
- Normale werden nach der Transformation automatisch normalisiert

## Zukünftige Erweiterungen

Mögliche Erweiterungen:

1. Quaternion-basierte Rotationen (besser für Interpolation)
2. Transform-Hierarchien (Parent-Child-Beziehungen)
3. Inverse Transformationen
4. Matrix-Caching für wiederholte Operationen
5. Animation-System basierend auf Transforms
6. Look-At Transformation
7. Orbit-Transformation um einen Punkt

## Integration mit GeometryGenerator

Neue Methoden wurden zu `GeometryGenerator` hinzugefügt:

### AddCubeWithTransform

```csharp
GeometryGenerator.AddCubeWithTransform(
    scene,
    new TransformData(
        new Vector3(5, 0, 0),       // Position
        new Vector3(0, MathF.PI/4, 0), // Rotation
        Vector3.One * 2              // Scale
    ),
    1.0f,                           // Size
    new Color(1, 0, 0)              // Color
);
```

### AddSphereWithTransform

```csharp
GeometryGenerator.AddSphereWithTransform(
    scene,
    new TransformData(
        new Vector3(0, 2, 0),
        Vector3.Zero,
        new Vector3(1, 2, 1)         // Ellipsoid (gestreckte Kugel)
    ),
    1.0f,                            // Radius
    12, 16,                          // Rings, Segments
    new Color(0, 1, 0),              // Color
    ShadingMode.Smooth
);
```

Diese Methoden erzeugen Geometrie um den Ursprung und wenden dann die Transformation an, was deutlich flexibler ist als
die ursprünglichen center-basierten Methoden.

## Naming-Konvention

- `Matrix4X4` statt `Matrix4x4` (folgt der Projekt-Konvention wie bei `Aabb`)
- `TransformData` statt `Transform` (vermeidet Namespace-Konflikt mit `Core.Scene.Transform`)
