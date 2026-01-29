# Entscheidung: Multi-Pass Rendering vs. Funktions-Refactoring

**Datum**: 2026-01-29  
**Kontext**: Analyse zur Reduzierung der Shader-Komplexität

## Problem

Der aktuelle `raytracing.comp` Shader hat 307 Zeilen und wird bei zusätzlichen Features (mehr Materialien, GI,
Denoising) schwerer wartbar.

## Geprüfte Lösung: Multi-Pass Rendering

### Konzept

- **Pass 1**: Primary Rays → G-Buffer (Position, Normal, Albedo, RayDir)
- **Pass 2**: Lighting + Shadows
- **Pass 3**: Reflections
- **Pass 4**: Composite + Post-Processing

### Vorteile

- ✅ Kleinere, fokussierte Shader (je ~150 Zeilen)
- ✅ Klare Verantwortlichkeiten
- ✅ Modulare Features (einzeln aktivierbar)
- ✅ Besseres Debugging (welcher Pass versagt?)

### Nachteile

- ❌ **~1000+ Zeilen C# Code** für Implementierung
- ❌ **6 zusätzliche Images** (G-Buffer + Intermediate)
- ❌ **4 neue Pipelines** + Shader Modules
- ❌ **4 neue Descriptor Set Layouts** + Sets
- ❌ **Command Buffer Refactoring** (4 Dispatches + Barriers)
- ❌ **Resize-Logic** für alle neuen Images
- ❌ **~353 MB VRAM** statt 14.7 MB (24x mehr!)
- ❌ **5-10% Performance-Verlust** durch Memory Bandwidth
- ❌ **4-6 Stunden Entwicklungszeit**
- ❌ **Hohes Fehlerrisiko** (schwer zu debuggen)

## Gewählte Lösung: Funktions-Refactoring

### Ansatz

Bestehenden Shader durch **Funktionen und Kommentare** organisieren, OHNE Multi-Pass.

```glsl
// ═══════════════════════════════════════
// INTERSECTION
// ═══════════════════════════════════════

Hit intersectTriangle(Ray ray, Triangle tri) { ... }
Hit trace(Ray ray, int numTriangles) { ... }
bool traceShadow(...) { ... }

// ═══════════════════════════════════════
// SHADING
// ═══════════════════════════════════════

vec3 calculateLighting(Hit hit, vec3 rayDir) { ... }
vec3 calculateReflections(Hit hit, vec3 rayDir) { ... }

// ═══════════════════════════════════════
// MAIN
// ═══════════════════════════════════════

void main() {
    Ray ray = generateRay();
    Hit hit = trace(ray, numTriangles);
    
    vec3 color = vec3(0.0);
    if (hit.hit) {
        color = calculateLighting(hit, ray.direction);
        color += calculateReflections(hit, ray.direction);
    }
    
    imageStore(outputImage, pixelCoords, vec4(color, 1.0));
}
```

### Vorteile

- ✅ **Gleiche Performance** wie aktuell
- ✅ **Keine C# Änderungen** nötig
- ✅ **Kein VRAM Overhead**
- ✅ **30 Minuten Aufwand** statt 6 Stunden
- ✅ **Niedriges Risiko**
- ✅ **Bessere Lesbarkeit** durch klare Funktionsnamen
- ✅ **Sofort nutzbar**

### Nachteile

- ❌ Shader bleibt in einer Datei (aber besser organisiert)
- ❌ Features nicht einzeln deaktivierbar (aber via #ifdef möglich)

## Begründung

**"Shader-Komplexität reduzieren" bedeutet NICHT zwingend "Multi-Pass Rendering".**

Multi-Pass ist ein **Architektur-Pattern** für komplexe Rendering-Pipelines, aber:

- Der aktuelle Shader hat nur 307 Zeilen
- Er ist bereits funktional getrennt (Intersection, Shading, Reflections)
- Das Problem ist **Organisation**, nicht **Komplexität**

Multi-Pass lohnt sich erst bei:

- **>500 Zeilen pro Shader**
- **Denoising** (braucht Temporal Accumulation)
- **Post-Processing** (Bloom, DOF, Motion Blur)
- **Temporal Anti-Aliasing**

## Entscheidung

**Jetzt**: Funktions-Refactoring (30 Min)  
**Später**: Multi-Pass wenn Features es erfordern (z.B. Denoising)

## Lessons Learned

1. **Premature Optimization**: Multi-Pass wäre Overengineering
2. **Cost-Benefit**: 6 Stunden vs. 30 Minuten für gleichen Nutzen
3. **YAGNI-Prinzip**: "You Aren't Gonna Need It" - Multi-Pass jetzt nicht nötig
4. **Inkrementelles Design**: Erst organisieren, dann bei Bedarf refactoren

## Referenz

- **[MULTI_SHADER_IMPLEMENTATION.md](./MULTI_SHADER_IMPLEMENTATION.md)** - Vollständige Analyse
- **[BILDAUSGABE_PIPELINE.md](./BILDAUSGABE_PIPELINE.md)** - Aktuelle Pipeline

---

**Fazit**: Die richtige Lösung zur richtigen Zeit. Multi-Pass ist nicht falsch, nur **jetzt nicht notwendig**.
