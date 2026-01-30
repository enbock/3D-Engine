﻿# Dokumentation - Vulkan 3D-Engine

## Übersicht

Dieses Verzeichnis enthält die gesamte technische Dokumentation des Projekts.

## Hauptdokumente

- **[ENTWICKLERTAGEBUCH.md](./ENTWICKLERTAGEBUCH.md)** - Vollständiges Entwicklertagebuch mit:
    - Projektübersicht & Architektur
    - Chronologische Implementierungs-Historie
    - Debugging-Sessions mit Lösungen
    - Technische Lösungen & Best Practices
    - Gelernte Lektionen
    - Aktueller Status & Roadmap

- **[REFACTORING_DEPENDENCY_INJECTION.md](./REFACTORING_DEPENDENCY_INJECTION.md)** - Container Isolation (✅
  Abgeschlossen):
  - Entfernung von Service Locator Anti-Pattern
  - Pure Constructor Injection für alle Use Cases
  - Container nur im Application Layer
  - Core Layer völlig unabhängig vom Container
  - Klare Dependencies & verbesserte Testbarkeit

- **[MULTI_PASS_IMPLEMENTATION_COMPLETE.md](./MULTI_PASS_IMPLEMENTATION_COMPLETE.md)** - Multi-Pass Rendering (✅
  Abgeschlossen):
  - 4-Pass Architektur (Primary Rays, Lighting, Reflections, Composite)
  - G-Buffer System (Position, Normal, Albedo, RayDir)
  - 6 zusätzliche Images (RGBA32F)
  - 4 spezialisierte Compute Shader
  - VulkanMultiPassTask Implementierung
  - Toggle zwischen Single-Pass / Multi-Pass
  - Performance Overhead: 307 MB VRAM (37x mehr)
  - Erfolgreich kompiliert und getestet

- **[MULTI_PASS_QUICKREF.md](./MULTI_PASS_QUICKREF.md)** - Multi-Pass Schnellreferenz:
  - Toggle zwischen Single/Multi-Pass
  - Architektur-Diagramm
  - G-Buffer Layout
  - Performance-Vergleich
  - Debugging-Tipps

- **[SESSION_SUMMARY_MULTI_PASS.md](./SESSION_SUMMARY_MULTI_PASS.md)** - Multi-Pass Session Log:
  - Implementierungs-Chronologie
  - Herausforderungen & Lösungen
  - Lessons Learned
  - Build-Ergebnisse

- **[SESSION_SUMMARY_COMPLEX_GEOMETRY.md](./SESSION_SUMMARY_COMPLEX_GEOMETRY.md)** - Komplexe Geometrie-Generierung (✅
  Abgeschlossen):
  - GeometryGenerator-Klasse
  - Zylinder, Kugel, Würfel aus Dreiecken
  - 430 Dreiecke statt 5
  - Prozedural generierte Geometrie

- **[BUGFIX_NORMALS_WINDING.md](./BUGFIX_NORMALS_WINDING.md)** - Normalen-Korrektur (✅ Behoben):
  - Invertierte Normalen bei Würfel, Kugel, Zylinder-Deckeln
  - Geometrie-Problem (nicht Shader)
  - Winding-Order auf CCW korrigiert
  - Erklärung der Cross-Product Richtung

- **[BUGFIX_CAMERA_DISTORTION.md](./BUGFIX_CAMERA_DISTORTION.md)** - Camera Verzerrung Fix (✅ Behoben):
  - Verzerrung bei vertikaler Kamerabewegung
  - Gimbal Lock Problem
  - Orthonormale Basis via Gram-Schmidt
  - Fix in beiden Shadern (Single-Pass & Multi-Pass)

- **[BUGFIX_CAMERA_CONTROLS.md](./BUGFIX_CAMERA_CONTROLS.md)** - Camera-Steuerung Korrektur (✅ Behoben):
  - Vollständig kamera-relative Steuerung implementiert
  - Q/E bewegen entlang camera.Up (nicht Welt-vertikal)

- **[PERFORMANCE_OPTIMIERUNG.md](./PERFORMANCE_OPTIMIERUNG.md)** - Performance-Analyse & Optimierung:
  - Analyse der FPS-Probleme bei 450+ Polygonen
  - Performance-Presets (Default, Performance, UltraPerformance)
  - BVH-Infrastruktur vorbereitet
  - Roadmap für weitere Optimierungen

- **[MULTI_PASS_STRATEGIE.md](./MULTI_PASS_STRATEGIE.md)** - Multi-Pass Rendering Entscheidung:
  - Warum Multi-Pass statt Single-Pass
  - 4-Pass Architektur mit BVH
  - Bindings pro Pass dokumentiert
  - Erweiterbarkeit für Soft Shadows, Caustics, Transparency
  - Alle Achsen (W/A/S/D/Q/E) sind kamera-relativ
  - "Fly Mode" Navigation wie in Blender

- **[BUGFIX_CAMERA_MOVEMENT_REFACTORING.md](./BUGFIX_CAMERA_MOVEMENT_REFACTORING.md)** - Camera-Bewegung nach
  Refactoring (✅ Behoben):
  - Forward-Vektor wurde falsch berechnet (aus camera.Forward statt pitch/yaw)
  - Separate Camera-Instanzen (WorldUseCase.camera vs scene.Camera)
  - Updates kamen nicht beim Renderer an
  - Lösung: Forward aus pitch/yaw berechnen, nur eine Camera-Instanz verwenden
  - Wichtige Lektion über Single Source of Truth

- **[REFACTORING_CAMERA_CONTROL.md](./REFACTORING_CAMERA_CONTROL.md)** - Camera Control Architektur-Refactoring (✅
  Abgeschlossen):
  - CameraControlUseCase von Application nach Core verschoben
  - Entkopplung von InputHandler durch Request-Objekte
  - CameraControlService im Application-Layer für Input-Translation
  - Clean Architecture Prinzipien korrekt angewendet
  - Dependency Inversion implementiert

- **[BILDAUSGABE_PIPELINE.md](./BILDAUSGABE_PIPELINE.md)** - Detaillierte Erklärung der Vulkan Render-Pipeline:
  - Phase 1: Initialisierung (Swapchain, Buffers, Pipeline, Synchronisation)
  - Phase 2: Render-Loop (Frame-by-Frame Ablauf mit allen Schritten)
  - Phase 3: Compute Shader Execution (GPU-seitige Raytracing-Berechnung)
  - Synchronisations-Mechanismen (Semaphoren, Fences, Image Layout Transitions)
  - Present-Mechanismus und VSync-Handling
  - Performance-Charakteristiken und Fehlerbehandlung

- **[MULTI_SHADER_IMPLEMENTATION.md](./MULTI_SHADER_IMPLEMENTATION.md)** - Multi-Pass Rendering Analyse (Geplant):
  - Konzept für Multi-Shader Architektur mit G-Buffer
  - Deferred Shading Ansatz für Raytracing
  - **Entscheidung**: Zu komplex, auf Eis gelegt
  - **Alternative**: Funktions-Refactoring im bestehenden Shader
  - Empfehlungen für zukünftige Shader-Organisation

- **[SHADER_REFACTORING_COMPLETE.md](./SHADER_REFACTORING_COMPLETE.md)** - Shader Funktions-Refactoring (✅
  Abgeschlossen):
  - Vollständiges Refactoring des raytracing.comp Shaders
  - main() von 47→16 Zeilen (-66%)
  - shade() von 50→16 Zeilen (-68%)
  - 7 neue spezialisierte Funktionen
  - Magic Numbers durch Konstanten ersetzt
  - Identische Performance, dramatisch bessere Lesbarkeit

- **[SHADER_CODE_COMPARISON.md](./SHADER_CODE_COMPARISON.md)** - Vorher/Nachher Vergleich:
  - Detaillierter Code-Vergleich des Refactorings
  - Metriken und Verbesserungen
  - Neue Funktions-Hierarchie
  - Konstanten-Eliminierung

- **[RENDERER_REFACTORING_TASKS.md](./RENDERER_REFACTORING_TASKS.md)** - Renderer Task-Architektur (✅ Abgeschlossen):
  - Zerlegung des 1450-Zeilen Renderers in 7 spezialisierte Task-Klassen
  - Separation of Concerns (SoC) Prinzip angewendet
  - VulkanDeviceTask, VulkanSwapchainTask, VulkanBufferTask, etc.
  - 70% weniger Zeilen pro Datei
  - Dramatisch verbesserte Wartbarkeit und Testbarkeit

## Archivierte Dokumente

Die ursprünglichen Einzeldokumente wurden in `archive/` verschoben und sind im Entwicklertagebuch zusammengefasst.

### Thematische Übersicht der archivierten Dokumente

#### Beleuchtung & Rendering

- `BELEUCHTUNG_ERFOLGREICH.md` - Initiale Beleuchtungs-Implementierung (hart codiert)
- `DYNAMISCHE_BELEUCHTUNG.md` - Dynamische Lichter mit 3 Typen
- `LIGHTING_IMPLEMENTATION.md` - Vollständige Beleuchtungs-Dokumentation
- `BACKFACE_CULLING_SCHATTEN.md` - Backface Culling & Schatten-Optimierung

#### Schatten-Techniken

- `POISSON_DISK_SHADOWS_2026-01-29.md` - Poisson Disk Sampling für weiche Schatten
- `PCSS_IMPLEMENTATION_2026-01-29.md` - PCSS (Percentage-Closer Soft Shadows)
- `SIMPLE_HARD_SHADOWS_2026-01-29.md` - Zurück zu einfachen harten Schatten

#### Debugging & Fehlerbehebung

- `DEBUG_SESSION.md` - 3-stündige Debug-Session zur Beleuchtung
- `FEHLERKORREKTUR_2026-01-28.md` - Korrekturen nach Rider-Absturz
- `DREIECK_KORREKTUR_2026-01-28.md` - Fehlendes mittleres Dreieck

#### Architektur & Refactoring

- `ARCHITECTURE_CORRECTION_2026-01-28.md` - Architektur-Korrekturen (leer)
- `FLAT_ARCHITECTURE_2026-01-28.md` - Flache Namespace-Struktur (leer)
- `NAMING_CONVENTION_2026-01-28.md` - Namenskonventionen (leer)
- `REFACTORING_2026-01-28.md` - Refactoring-Dokumentation (leer)
- `USECASE_STRUCTURE_2026-01-28.md` - Use-Case Struktur (leer)

## Verwendung

### Für neue Entwickler

Starte mit **ENTWICKLERTAGEBUCH.md** - es enthält alles Wichtige chronologisch aufbereitet.

### Für spezifische Themen

Nutze das Inhaltsverzeichnis im Entwicklertagebuch, um direkt zum relevanten Abschnitt zu springen.

### Für historische Details

Die archivierten Dokumente enthalten teilweise mehr Details zu einzelnen Debugging-Sessions.

## Struktur

```
doc/
├── README.md                         # Diese Datei
├── ENTWICKLERTAGEBUCH.md             # Hauptdokumentation
├── BILDAUSGABE_PIPELINE.md           # Vulkan Render-Pipeline Erklärung
├── MULTI_SHADER_IMPLEMENTATION.md    # Multi-Pass Rendering Analyse (Geplant)
├── ENTSCHEIDUNG_MULTI_PASS.md        # Entscheidungsdokumentation Multi-Pass
├── SHADER_REFACTORING_COMPLETE.md    # Shader Refactoring Abschluss
├── SHADER_CODE_COMPARISON.md         # Vorher/Nachher Vergleich
├── RENDERER_REFACTORING_TASKS.md     # Renderer Task-Architektur
├── SESSION_SUMMARY_MULTI_SHADER.md   # Session-Zusammenfassung
└── archive/                          # Archivierte Einzeldokumente
    ├── BELEUCHTUNG_ERFOLGREICH.md
    ├── DYNAMISCHE_BELEUCHTUNG.md
    ├── LIGHTING_IMPLEMENTATION.md
    ├── BACKFACE_CULLING_SCHATTEN.md
    ├── POISSON_DISK_SHADOWS_2026-01-29.md
    ├── PCSS_IMPLEMENTATION_2026-01-29.md
    ├── SIMPLE_HARD_SHADOWS_2026-01-29.md
    ├── DEBUG_SESSION.md
    ├── FEHLERKORREKTUR_2026-01-28.md
    ├── DREIECK_KORREKTUR_2026-01-28.md
    ├── ARCHITECTURE_CORRECTION_2026-01-28.md
    ├── FLAT_ARCHITECTURE_2026-01-28.md
    ├── NAMING_CONVENTION_2026-01-28.md
    ├── REFACTORING_2026-01-28.md
    └── USECASE_STRUCTURE_2026-01-28.md
```

## Wartung

Bei neuen Features oder Bugfixes:

1. Update **ENTWICKLERTAGEBUCH.md** im entsprechenden Abschnitt
2. Chronologische Einträge unter "Implementierungs-Chronologie" hinzufügen
3. Gelernte Lektionen unter "Gelernte Lektionen" dokumentieren
4. "Aktueller Status" aktualisieren

## Prinzipien

Diese Dokumentation folgt den gleichen Prinzipien wie der Code:

- **Clean**: Klar strukturiert, leicht zu navigieren
- **Vollständig**: Alle Entscheidungen dokumentiert
- **Ehrlich**: Fehler und Lösungen gleichermaßen dokumentiert
- **Aktuell**: Status immer up-to-date

---

**Letzte Aktualisierung**: 2026-01-30
