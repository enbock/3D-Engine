# 3D Engine

Eine 3D-Engine entwickelt mit TypeScript und WebGL, unter Verwendung von Clean Architecture und DDD-Prinzipien.

## Architektur

Das Projekt folgt den Prinzipien der Clean Architecture mit folgenden Schichten:

- **Core**: Domain-Modelle und Geschäftslogik
- **Application**: Use Cases und Anwendungsservices
- **Infrastructure**: Implementierungsdetails (Rendering, Input, etc.)

## Erste Schritte

### Installation

```bash
npm install
```

### Entwicklung

Startet den Development Server mit Hot Reload:

```bash
npm run dev
```

Der Server läuft auf `http://localhost:9000`

### Build

Erstellt eine produktionsreife Version:

```bash
npm run build
```

### Watch Mode

Automatisches Neu-Kompilieren bei Änderungen:

```bash
npm run watch
```

## Projektstruktur

```
3D-Engine/
├── public/                 # Statische Dateien
│   └── index.html         # HTML Starter-Seite
├── src/                   # TypeScript Quellcode
│   ├── Core/              # Domain Layer
│   │   └── Engine.ts      # Core Engine (WebGL, Rendering)
│   ├── Application/       # Application Layer
│   │   ├── EngineConfig.ts          # Engine Konfiguration
│   │   └── Engine/
│   │       └── EngineController.ts  # Engine Controller
│   ├── Infrastructure/    # Infrastructure Layer
│   │   └── index.ts      # (Bereit für Implementierungen)
│   └── main.ts           # Entry Point
├── dist/                 # Build Output
├── package.json          # NPM Konfiguration
├── tsconfig.json         # TypeScript Konfiguration
└── webpack.config.js     # Webpack Konfiguration
```

## Technologie-Stack

- **TypeScript**: Typsichere Entwicklung
- **Webpack**: Module Bundler
- **WebGL**: 3D Rendering
- **Webpack Dev Server**: Entwicklungsserver mit Hot Reload

## Entwicklungsprinzipien

- Clean Code
- Clean Architecture
- Domain-Driven Design (DDD)
- Separation of Concerns (SoC)

