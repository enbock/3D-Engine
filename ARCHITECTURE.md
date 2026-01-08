# 3D Engine - Clean Architecture Übersicht

## Layer-Architektur

```
┌─────────────────────────────────────────────────────────────┐
│                        UI Layer                             │
│                      (main.ts)                              │
│  - Application Bootstrap                                    │
│  - DOM Manipulation                                         │
│  - Event Handling                                           │
└────────────────────┬────────────────────────────────────────┘
                     │ depends on
                     ↓
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ EngineController                                     │  │
│  │  - initialize()                                      │  │
│  │  - start() / stop()                                  │  │
│  │  - resize()                                          │  │
│  │  - isRunning()                                       │  │
│  │  - dispose()                                         │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ EngineConfig                                         │  │
│  │  - canvas: HTMLCanvasElement                         │  │
│  │  - width, height                                     │  │
│  │  - antialias, powerPreference                        │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────┬────────────────────────────────────────┘
                     │ orchestrates
                     ↓
┌─────────────────────────────────────────────────────────────┐
│                      Core Layer                             │
│                    (Domain Logic)                           │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Engine (Aggregate Root)                              │  │
│  │  - initializeContext()                               │  │
│  │  - renderLoop()                                      │  │
│  │  - update() / render()                               │  │
│  │  - WebGL Context Management                          │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                     ↑
                     │ will use
                     │
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                       │
│               (External Dependencies)                       │
│                                                             │
│  - Rendering (Shader Management)                            │
│  - Input (Keyboard, Mouse, Touch)                           │
│  - Assets (Texture, Model Loading)                          │
│  - External Services                                        │
└─────────────────────────────────────────────────────────────┘
```

## Datenfluss

### Initialisierung:
```
main.ts
  → erstellt EngineConfig
  → erstellt EngineController(config)
  → ruft engineController.initialize()
    → erstellt Engine(config)
    → ruft engine.initialize()
      → initialisiert WebGL Context
      → startet Render Loop
```

### Resize-Event:
```
window.resize Event
  → main.ts handleResize()
  → engineController.resize(width, height)
    → engine.resize(width, height)
      → aktualisiert WebGL Viewport
```

### Render Loop (läuft kontinuierlich):
```
requestAnimationFrame
  → engine.renderLoop()
    → engine.update(deltaTime)    // Logik-Updates
    → engine.render()             // WebGL Rendering
    → requestAnimationFrame       // Nächster Frame
```

## Dependency Direction (Abhängigkeitsrichtung)

```
UI Layer (main.ts)
    ↓
Application Layer (EngineController, EngineConfig)
    ↓
Core Layer (Engine)
    ↑
Infrastructure Layer (future: Shaders, Input, Assets)
```

**Wichtig:** Abhängigkeiten zeigen immer nach INNEN (zum Core).
Der Core kennt keine äußeren Schichten!

## SOLID Principles

### Single Responsibility:
- **Engine**: Nur WebGL und Rendering
- **EngineController**: Nur Orchestrierung
- **main.ts**: Nur UI-Initialisierung

### Open/Closed:
- Erweiterbar durch neue Controller im Application Layer
- Core bleibt unverändert

### Liskov Substitution:
- Engine könnte durch andere Rendering-Engines ersetzt werden
- Interface bleibt gleich

### Interface Segregation:
- Kleine, fokussierte Interfaces
- EngineConfig enthält nur notwendige Konfiguration

### Dependency Inversion:
- High-level Module (UI) hängen von Abstraktionen ab (Controller)
- Low-level Module (Engine) sind unabhängig

## DDD Concepts

### Aggregate Root:
- **Engine** ist das Aggregate Root für das Rendering-System
- Schützt seine internen Invarianten
- Bietet klare öffentliche API

### Value Objects:
- **EngineConfig** ist ein Value Object
- Immutable Configuration

### Application Services:
- **EngineController** ist ein Application Service
- Koordiniert Domain-Objekte
- Implementiert Use Cases

## Vorteile dieser Architektur

✅ **Testbarkeit**: Jede Schicht kann isoliert getestet werden
✅ **Wartbarkeit**: Klare Trennung der Verantwortlichkeiten
✅ **Erweiterbarkeit**: Neue Features ohne Core-Änderungen
✅ **Flexibilität**: Einfacher Austausch von Komponenten
✅ **Verständlichkeit**: Klare Struktur und Abhängigkeiten

