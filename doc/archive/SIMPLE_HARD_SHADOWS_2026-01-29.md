# Zurück zu einfachen harten Schatten

**Datum:** 2026-01-29

## Problem

Die PCSS-Implementierung (Percentage-Closer Soft Shadows) hat nicht wie erwartet funktioniert:

- Keine sichtbare Verbesserung
- Komplexität ohne Nutzen
- Performance-Kosten ohne Qualitätsgewinn

## Lösung

Alle weichen Schatten-Features entfernt und zurück zu **einfachen harten Schatten**:

### Was wurde entfernt:

1. **Poisson Disk Array** (64 Sample-Punkte)
2. **findBlockerDistance()** - PCSS Blocker Search
3. **Komplexes traceShadow()** - Multi-Sample Filtering
4. **traceShadowRay()** - Helper für Hit-Return

### Neue einfache Implementierung:

```glsl
bool traceShadow(vec3 origin, vec3 lightDir, float maxDist, int numTriangles) {
    if (settings.enableShadows == 0) {
        return false;
    }

    Ray shadowRay;
    shadowRay.origin = origin;
    shadowRay.direction = lightDir;

    for (int i = 0; i < numTriangles; i++) {
        Hit h = intersectTriangleShadow(shadowRay, triangles[i]);
        if (h.hit && h.dist < maxDist) {
            return true;  // Schatten gefunden
        }
    }

    return false;  // Kein Schatten
}
```

### Verwendung in shade():

```glsl
bool inShadow = traceShadow(hit.point + normal * EPSILON * 10.0, lDir, dist, numTriangles);
float shadowFactor = inShadow ? 0.2 : 1.0;

result += color * lightColor * diff * light.intensity * shadowFactor;
```

## Eigenschaften

- **Binär**: Entweder im Schatten (20% Helligkeit) oder nicht (100% Helligkeit)
- **Hart**: Keine weichen Übergänge
- **Schnell**: Nur ein Shadow Ray pro Lichtquelle
- **Einfach**: Keine komplexen Berechnungen

## Performance

- **Vorher (PCSS)**: ~16-64 Shadow Rays pro Pixel pro Licht
- **Jetzt**: 1 Shadow Ray pro Pixel pro Licht
- **Verbesserung**: ~16-64x schneller

## Nächste Schritte

Falls weiche Schatten gewünscht:

1. Problem mit PCSS analysieren (warum funktioniert es nicht?)
2. Alternative Methoden recherchieren
3. Einfachere Soft Shadow Techniken testen (z.B. fixed-size Poisson ohne PCSS)

## Dateien geändert

- `Infrastructure/Rendering/Vulkan/Shaders/raytracing.comp`
    - Entfernt: Poisson Disk Array, findBlockerDistance(), komplexes traceShadow()
    - Vereinfacht: Neues traceShadow() gibt bool zurück
    - Angepasst: shade() verwendet shadowFactor statt direkten float-Wert
