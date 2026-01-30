# Refactoring: CameraControl in Core verschoben

**Datum:** 2026-01-30

## Änderung

Die Kamera-Steuerungslogik wurde von der Application-Layer in die Core-Layer verschoben und dabei von
Input-Abhängigkeiten entkoppelt.

## Motivation

Die ursprüngliche Implementierung hatte folgende Probleme:

- `CameraControlUseCase` war im **Application-Layer** (falsch nach Clean Architecture)
- Der UseCase hatte eine direkte Abhängigkeit zu `InputHandler` (Verletzung der Dependency Inversion)
- Core-Logik war mit Infrastructure-Details vermischt

## Durchgeführte Änderungen

### 1. Request-Objekte erstellt (Core/CameraControl/)

**UpdateCameraMovementRequest.cs**

```csharp
public class UpdateCameraMovementRequest
{
    public Vector3 Movement { get; set; }
    public float DeltaTime { get; set; }
}
```

**UpdateCameraLookRequest.cs**

```csharp
public class UpdateCameraLookRequest
{
    public Vector3 MouseDelta { get; set; }
}
```

### 2. UseCase in Core verschoben (Core/CameraControl/)

**CameraControlUseCase.cs**

- Verschoben von `Application/CameraControl/` nach `Core/CameraControl/`
- Entfernt: Abhängigkeit zu `InputHandler`
- Geändert: Konstruktor nimmt nur noch `CameraEntity` entgegen
- Neue Methoden:
    - `UpdateMovement(UpdateCameraMovementRequest request)`
    - `UpdateLook(UpdateCameraLookRequest request)`
- Alte Methode entfernt: `Run(float deltaTime)`

### 3. Service im Application-Layer erstellt

**CameraControlService.cs** (Application/CameraControl/)

- Verbindet `InputHandler` mit `CameraControlUseCase`
- Übersetzt Input-Daten in Request-Objekte
- Methode: `Update(float deltaTime)`

### 4. UpdateEngineUseCase aktualisiert

**Core/EngineUpdate/UpdateEngineUseCase.cs**

- Verwendet nun `CameraControlService` statt `CameraControlUseCase`
- Ruft `service.Update(deltaTime)` auf

## Architektur-Verbesserungen

### Vorher (❌ Falsch)

```
Application Layer:
  - CameraControlUseCase (mit InputHandler-Abhängigkeit)
     ↓
Core Layer:
  - UpdateEngineUseCase → CameraControlUseCase
```

### Nachher (✅ Korrekt)

```
Application Layer:
  - InputHandler → CameraControlService → CameraControlUseCase (Request)
     ↓                                            ↓
Core Layer:                                       ↓
  - UpdateEngineUseCase → CameraControlService    ↓
  - CameraControlUseCase ← UpdateCameraMovementRequest
                         ← UpdateCameraLookRequest
```

## Clean Architecture Prinzipien eingehalten

✅ **Separation of Concerns (SoC)**

- Core: Reine Geschäftslogik (Kamera-Mathematik)
- Application: Orchestrierung (Input → Request-Transformation)

✅ **Dependency Inversion**

- Core kennt Application nicht
- Core kennt InputHandler nicht
- Communication über Request-Objekte

✅ **Single Responsibility**

- `CameraControlUseCase`: Nur Kamera-Logik
- `CameraControlService`: Nur Input-zu-Request-Übersetzung

## Technische Details

### Verwendete Patterns

- **Request-Response Pattern**: UseCase kommuniziert über DTOs
- **Service Pattern**: Application-Service orchestriert Core-UseCase
- **Dependency Injection**: Container verwaltet Abhängigkeiten

### Math-Fix

- `Math.Clamp()` ersetzt durch `Math.Max(min, Math.Min(max, value))`
- Grund: Kompatibilität mit älteren .NET-Versionen

## Ergebnis

✅ Build erfolgreich
✅ Keine Compiler-Fehler
✅ Clean Architecture eingehalten
✅ Dependency Inversion korrekt implementiert

## Gelerntes

1. **UseCases gehören IMMER in Core**, nie in Application
2. **UseCases dürfen KEINE Infrastructure-Abhängigkeiten haben** (wie InputHandler)
3. **Request-Objekte sind der Schlüssel** zur Entkopplung
4. **Application-Services übersetzen** Infrastructure → Core-Requests
