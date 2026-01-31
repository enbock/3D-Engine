# Teapot Normal-Debug Session - GELÖST ✅

## Problem

Die Teekanne wurde uniform weiß (von oben) oder schwarz (von unten) gerendert, mit einer scharfen Kante dazwischen. Das
deutete darauf hin, dass die Normalen nicht der Oberfläche folgten.

## Ursache gefunden: Fehlerhafte Normalen im glTF-Modell

Die Normalen im `teapot.glb` Modell waren fehlerhaft - sie zeigten alle in die gleiche Richtung statt der Oberfläche zu
folgen.

### Zusätzliches Problem: Transform-Matrix

`Vector3.TransformNormal(normals[i], transform)` wendete die World-Matrix auf Normalen an, was sie weiter beschädigte.

## Lösung: Face-Normalen im ModelLoader berechnen

### Änderung im ModelLoader

```csharp
public class ModelLoader(..., bool calculateNormals = true)
{
    private void CalculateFaceNormals(MeshData meshData)
    {
        for (int i = 0; i < meshData.Indices.Count; i += 3)
        {
            Vector3 p0 = meshData.Vertices[idx0].Position;
            Vector3 p1 = meshData.Vertices[idx1].Position;
            Vector3 p2 = meshData.Vertices[idx2].Position;

            Vector3 edge1 = p1 - p0;
            Vector3 edge2 = p2 - p0;
            Vector3 faceNormal = Vector3.Cross(edge1, edge2).Normalized;

            // Setze Face-Normale für alle 3 Vertices
            meshData.Vertices[idx0] = new VertexData { ..., Normal = faceNormal };
            meshData.Vertices[idx1] = new VertexData { ..., Normal = faceNormal };
            meshData.Vertices[idx2] = new VertexData { ..., Normal = faceNormal };
        }
    }
}
```

### Konfiguration

- `calculateNormals = true` (default): Berechnet Face-Normalen aus Vertices
- `calculateNormals = false`: Verwendet Normalen aus dem glTF-Modell

## Ergebnis

- ✅ Teekanne wird korrekt beleuchtet
- ✅ Flat Shading (facettierte Oberfläche)
- ✅ Licht-zugewandte Seiten hell, abgewandte dunkel

## Zusätzliche Shader-Fixes (beibehalten)

1. **Doppelseitige Dreiecke**: `if (abs(det) < EPSILON)`
    - Erlaubt Ray-Hits auf beiden Seiten eines Dreiecks

2. **Automatisches Normalen-Flippen**:
   ```glsl
   if (dot(interpNormal, ray.direction) > 0.0) {
       interpNormal = -interpNormal;
   }
   ```
    - Korrigiert Normale wenn sie vom Ray weg zeigt

## Lesson Learned

1. **glTF-Normalen nicht blind vertrauen** - Modelle können fehlerhafte Normalen haben
2. **Face-Normalen berechnen** ist eine sichere Fallback-Lösung
3. **TransformNormal** auf Normalen anwenden erfordert die inverse Transpose Matrix
4. **Flat Shading** als erstes testen, dann Smooth Shading hinzufügen

