# Architektur-Refactoring: Clean Architecture Implementation

## Datum: 2026-01-08

## Durchgeführte Änderungen

### 1. **EngineConfig nach Application Layer verschoben**

**Vorher:**
```
src/Core/Types/EngineConfig.ts
```

**Nachher:**
```
src/Application/EngineConfig.ts
```

**Begründung:** Die Konfiguration gehört zur Application Layer, da sie die Schnittstelle zwischen dem UI (main.ts) und der Core-Domain darstellt.

---

### 2. **EngineController in Application Layer erstellt**

**Neue Datei:**
```
src/Application/Engine/EngineController.ts
```

**Verantwortlichkeiten des Controllers:**
- Orchestriert den Engine-Lebenszyklus
- Koordiniert zwischen den Layern (Application ↔ Core)
- Verwaltet die Initialisierung, Updates und Resource Management
- Bietet eine saubere API für die UI-Schicht

**Öffentliche Methoden:**
- `initialize()` - Initialisiert die Engine
- `start()` - Startet die Render-Loop
- `stop()` - Stoppt die Render-Loop
- `resize(width, height)` - Passt Viewport an
- `getFPS()` - Gibt aktuelle FPS zurück
- `isRunning()` - Prüft ob Engine läuft
- `dispose()` - Gibt Ressourcen frei
- `getEngine()` - Gibt Engine-Instanz zurück (für erweiterte Nutzung)

---

### 3. **Core Engine bereinigt**

**Änderungen:**
- Import von `EngineConfig` zeigt nun auf Application Layer
- Property `isRunning` umbenannt zu `running` (um Namenskonflikt mit Methode zu vermeiden)
- Neue public Methode `isRunning()` hinzugefügt
- Engine fokussiert sich jetzt rein auf WebGL-Rendering und Domain-Logik

**Core Engine bleibt verantwortlich für:**
- WebGL Context Management
- Render Loop
- Update/Render Cycle
- Viewport Management
- Resource Cleanup

---

### 4. **main.ts aktualisiert**

**Vorher:**
```typescript
import { Engine } from './Core/Engine';
// ...
this.engine = new Engine(config);
await this.engine.initialize();
```

**Nachher:**
```typescript
import { EngineController } from './Application/Engine/EngineController';
// ...
this.engineController = new EngineController(config);
await this.engineController.initialize();
```

**Begründung:** Die UI-Schicht kommuniziert nun über den Controller mit der Core Engine, was die Abhängigkeiten korrekt invertiert.

---

## Clean Architecture Prinzipien

### Dependency Flow:
```
main.ts (UI/Entry Point)
    ↓
EngineController (Application Layer)
    ↓
Engine (Core/Domain Layer)
```

### Vorteile dieser Struktur:

1. **Separation of Concerns (SoC)**
   - Core: Reine Geschäftslogik (WebGL, Rendering)
   - Application: Orchestrierung und Use Cases
   - UI: Nur Initialisierung und User Input

2. **Dependency Inversion**
   - Core Layer kennt Application Layer nicht
   - Application Layer orchestriert Core Layer
   - UI-Layer kennt nur Application Layer

3. **Testbarkeit**
   - Core Engine kann isoliert getestet werden
   - Controller kann mit Mock-Engine getestet werden
   - UI kann mit Mock-Controller getestet werden

4. **Erweiterbarkeit**
   - Neue Features können im Application Layer hinzugefügt werden
   - Core bleibt stabil und fokussiert
   - Einfaches Hinzufügen von Use Cases

---

## Projektstruktur (Aktualisiert)

```
src/
├── Core/                              # Domain Layer
│   └── Engine.ts                     # Core Engine (WebGL, Rendering)
│
├── Application/                       # Application Layer
│   ├── EngineConfig.ts               # Engine Konfiguration
│   └── Engine/
│       └── EngineController.ts       # Engine Controller (Orchestrierung)
│
├── Infrastructure/                    # Infrastructure Layer
│   └── index.ts                      # (Bereit für zukünftige Implementierungen)
│
└── main.ts                           # Entry Point (UI Layer)
```

---

## Nächste Schritte

Folgende Komponenten sollten in Zukunft hinzugefügt werden:

### Application Layer:
- `SceneController.ts` - Szenen-Management
- `CameraController.ts` - Kamera-Steuerung
- `InputController.ts` - Input-Handling-Orchestrierung

### Infrastructure Layer:
- `Rendering/ShaderManager.ts` - Shader-Verwaltung
- `Input/KeyboardHandler.ts` - Tastatur-Input
- `Input/MouseHandler.ts` - Maus-Input
- `Assets/TextureLoader.ts` - Textur-Laden
- `Assets/ModelLoader.ts` - Model-Laden

### Core Layer:
- `Scene.ts` - Szenen-Graph
- `Camera.ts` - Kamera-Domain-Logik
- `Mesh.ts` - Mesh/Geometry
- `Material.ts` - Material-System

---

## Status: ✅ ERFOLGREICH REFACTORED

Alle Änderungen wurden erfolgreich implementiert und getestet.
Build läuft ohne Fehler durch!

