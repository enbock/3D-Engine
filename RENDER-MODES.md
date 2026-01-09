# Render-Modus Wechsel

## Überblick
Die 3D-Engine unterstützt jetzt zwei Rendering-Modi:
- **Raytracing**: GPU-basiertes Raytracing mit Schatten und Reflexionen
- **Raster**: Klassisches polygonales Rendering mit Vertex/Fragment-Shadern

## Implementierung

### Neue Dateien:
1. **RasterRenderer.ts**: Klassischer Raster-Renderer
   - Vertex-/Fragment-Shader
   - Buffer-Management für Meshes
   - Blinn-Phong Beleuchtung
   - Depth-Testing & Backface-Culling

2. **RendererManager.ts**: Verwaltet beide Renderer
   - Wechselt zwischen Modi
   - Einheitliche Render-Schnittstelle
   - Automatisches Dispose beider Renderer

### Änderungen:
- **Engine.ts**: Nutzt RendererManager statt direkten Renderer
- **EngineController.ts**: Expose Render-Modus-Methoden
- **main.ts**: Button-Event-Handler zum Wechseln
- **index.html**: UI-Button mit Styling

## Verwendung

### Programmgesteuert:
```typescript
engineController.setRenderMode(RenderMode.Raytracing);
engineController.setRenderMode(RenderMode.Raster);
```

### UI-Button:
Klicke auf "Zu Raster wechseln" / "Zu Raytracing wechseln" oben rechts.

## Unterschiede

### Raytracing:
- ✅ Realistische Schatten
- ✅ Reflexionen
- ✅ Accurate Beleuchtung
- ❌ Langsamer (GPU Fragment Shader)
- ❌ Triangle-Limit (~87,000)

### Raster:
- ✅ Sehr schnell
- ✅ Unbegrenzte Dreiecke
- ✅ Hardware-beschleunigt
- ❌ Keine Reflexionen
- ❌ Einfachere Schatten

## Performance
Der Wechsel erfolgt in Echtzeit ohne Neustart der Engine.

