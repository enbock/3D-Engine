# Bildausgabe-Pipeline der Vulkan Raytracing Engine

## Übersicht

Diese Dokumentation beschreibt detailliert die Schritte der Bildausgabe in der Vulkan-basierten 3D-Raytracing-Engine.
Der Prozess nutzt Vulkan Compute Shaders für das Raytracing und präsentiert das Ergebnis über eine Swapchain auf dem
Bildschirm.

---

## Architektur-Überblick

```
Program.cs
    ↓
EngineController
    ↓
VulkanRenderer (Wrapper)
    ↓
InternalVulkanRenderer (Vulkan-Implementierung)
    ↓
GPU (Compute Shader) → Swapchain → Monitor
```

---

## Phase 1: Initialisierung (Einmalig beim Start)

### 1.1 Engine-Start

**Datei**: `Program.cs`

- Konfiguration erstellen (Fenstergröße: 2560x1440, VSync aktiviert)
- `EngineController` initialisieren
- Engine starten mit `engine.Run()`

### 1.2 Vulkan-Initialisierung

**Datei**: `InternalVulkanRenderer.cs` → `Initialize()`

#### 1.2.1 Instance & Device

- Vulkan Instance erstellen
- Physical Device auswählen (z.B. NVIDIA RTX 3070)
- Logical Device erstellen
- Compute Queue & Present Queue holen

#### 1.2.2 Window Surface

- Window Surface für das Betriebssystem erstellen
- Surface mit Vulkan verknüpfen

#### 1.2.3 Swapchain erstellen

**Wichtig**: Die Swapchain ist der Mechanismus zum Darstellen von Bildern

```
Swapchain Configuration:
- Format: B8G8R8A8_SRGB (32-bit Farbe mit sRGB)
- Present Mode: FIFO (VSync) oder IMMEDIATE
- Image Count: Min. 3 Bilder (Triple Buffering)
- Usage: Transfer Destination + Color Attachment
- Extent: 2560x1440 (Fenstergröße)
```

**Was passiert**:

- Vulkan erstellt mehrere Images (typisch 3) in der Swapchain
- Diese Images werden rotierend verwendet
- Jedes Image benötigt einen ImageView für den Zugriff

#### 1.2.4 Storage Image

- Separates Image für den Compute Shader Output
- Format: R32G32B32A32_SFLOAT (HDR-fähig)
- Layout: GENERAL (für Compute Shader read/write)
- Größe: Identisch zur Swapchain

#### 1.2.5 Buffers erstellen

**GPU-Speicher für Szenen-Daten**:

1. **Camera Buffer**: Kamera-Position, Ziel, FOV, Zeit
2. **Triangle Buffer**: Alle Dreiecke der Szene (Vertices, Normalen, Farben)
3. **Light Buffer**: Lichtquellen (Position, Farbe, Intensität, Typ)
4. **Settings Buffer**: Render-Einstellungen (Bounces, Schatten, Reflections)

#### 1.2.6 Compute Pipeline

- Shader laden (`raytracing.comp.spv`)
- Descriptor Set Layout (5 Bindings für alle Buffer/Images)
- Pipeline Layout erstellen
- Compute Pipeline kompilieren

#### 1.2.7 Synchronisations-Objekte

**Kritisch für korrekte Vulkan-Synchronisation**:

```csharp
imageAvailableSemaphores[MaxFramesInFlight]     // 2 Semaphoren
renderFinishedSemaphores[SwapchainImageCount]   // 3 Semaphoren (!)
inFlightFences[MaxFramesInFlight]               // 2 Fences
```

**Wichtig**: `renderFinishedSemaphores` hat einen Eintrag pro Swapchain-Image, nicht pro Frame!

---

## Phase 2: Render-Loop (Jeder Frame)

### 2.1 Frame-Start

**Datei**: `EngineController.cs` → `OnRender()`

- `RenderEngineUseCase` wird aufgerufen
- Ruft `VulkanRenderer.Render()` auf
- Ruft `InternalVulkanRenderer.Render()` auf

### 2.2 CPU-GPU Synchronisation

**Datei**: `InternalVulkanRenderer.cs` → `Render()`

#### Schritt 1: Auf GPU warten

```csharp
_vk.WaitForFences(_device, 1, &_inFlightFences[_currentFrame], true, ulong.MaxValue);
_vk.ResetFences(_device, 1, &_inFlightFences[_currentFrame]);
```

**Was passiert**: CPU wartet, bis GPU den vorherigen Frame fertig verarbeitet hat.

#### Schritt 2: Swapchain-Image akquirieren

```csharp
_khrSwapchain.AcquireNextImage(_device, _swapchain, ulong.MaxValue,
    _imageAvailableSemaphores[_currentFrame], default, &imageIndex);
```

**Was passiert**:

- Fragt Swapchain nach dem nächsten verfügbaren Image
- Signalisiert `imageAvailableSemaphores[_currentFrame]` wenn bereit
- Gibt `imageIndex` zurück (0, 1, oder 2 bei Triple Buffering)

### 2.3 Uniform Buffers aktualisieren

**Methode**: `UpdateUniformBuffers()`

#### Kamera-Daten

```csharp
CameraUniformData {
    Position: vec3(x, y, z)
    Target: vec3(x, y, z)
    Resolution: vec2(width, height)
    Time: float
    Fov: float
}
```

#### Licht-Daten

- Bis zu 8 Lichtquellen
- Jedes Licht: Typ, Position, Richtung, Farbe, Intensität

#### Render-Settings

- Max Bounces, Shadow Samples, Shadow Softness
- Enable Shadows/Reflections, Reflection Strength

**Was passiert**: CPU schreibt aktuelle Szenen-Daten in GPU-Buffer.

### 2.4 Command Buffer aufzeichnen

**Methode**: `RecordCommandBuffer()`

#### Schritt 1: Compute Shader dispatchen

```csharp
_vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, _computePipeline);
_vk.CmdBindDescriptorSets(...);

uint groupCountX = (width + 15) / 16;   // 160 Gruppen bei 2560px
uint groupCountY = (height + 15) / 16;  // 90 Gruppen bei 1440px
_vk.CmdDispatch(commandBuffer, groupCountX, groupCountY, 1);
```

**Was passiert**:

- Compute Shader wird gestartet mit 16x16 Pixel pro Work-Group
- Jeder Pixel wird parallel berechnet (Raytracing)
- Output: Storage Image mit gerenderten Pixeln

#### Schritt 2: Image Layout Transitions

**a) Storage Image vorbereiten**

```
GENERAL → TRANSFER_SRC_OPTIMAL
```

- Pipeline Barrier: Compute Shader schreibt fertig → Transfer kann lesen

**b) Swapchain Image vorbereiten**

```
UNDEFINED → TRANSFER_DST_OPTIMAL
```

- Pipeline Barrier: Swapchain-Image bereit für Kopieren

#### Schritt 3: Image kopieren

```csharp
_vk.CmdCopyImage(commandBuffer, 
    _storageImage, ImageLayout.TransferSrcOptimal,
    _swapchainImages[imageIndex], ImageLayout.TransferDstOptimal,
    1, &copyRegion);
```

**Was passiert**: Storage Image (Raytracing-Ergebnis) wird in Swapchain-Image kopiert.

#### Schritt 4: Finale Layout Transitions

**a) Swapchain-Image für Präsentation**

```
TRANSFER_DST_OPTIMAL → PRESENT_SRC_KHR
```

- Swapchain-Image ist bereit zur Anzeige

**b) Storage Image zurücksetzen**

```
TRANSFER_SRC_OPTIMAL → GENERAL
```

- Storage Image bereit für nächsten Frame

### 2.5 Command Buffer absenden

```csharp
var submitInfo = new SubmitInfo {
    WaitSemaphoreCount = 1,
    PWaitSemaphores = &_imageAvailableSemaphores[_currentFrame],  // Warte auf Image
    PWaitDstStageMask = &PipelineStageFlags.ComputeShaderBit,
    CommandBufferCount = 1,
    PCommandBuffers = &commandBuffer,
    SignalSemaphoreCount = 1,
    PSignalSemaphores = &_renderFinishedSemaphores[imageIndex]    // Signalisiere fertig
};

_vk.QueueSubmit(_computeQueue, 1, &submitInfo, _inFlightFences[_currentFrame]);
```

**Was passiert**:

- Command Buffer wird an GPU-Queue gesendet
- GPU wartet auf `imageAvailableSemaphores[_currentFrame]`
- Nach Fertigstellung: GPU signalisiert `renderFinishedSemaphores[imageIndex]`
- Fence wird gesetzt wenn GPU fertig ist

### 2.6 Bild präsentieren

```csharp
var presentInfo = new PresentInfoKHR {
    WaitSemaphoreCount = 1,
    PWaitSemaphores = &_renderFinishedSemaphores[imageIndex],  // Warte auf Rendering
    SwapchainCount = 1,
    PSwapchains = &_swapchain,
    PImageIndices = &imageIndex
};

_khrSwapchain.QueuePresent(_presentQueue, &presentInfo);
```

**Was passiert**:

- Present-Operation wartet auf `renderFinishedSemaphores[imageIndex]`
- Swapchain-Image wird auf dem Monitor angezeigt
- VSync (falls aktiviert) synchronisiert mit Monitor-Refresh-Rate

### 2.7 Frame-Counter erhöhen

```csharp
_currentFrame = (_currentFrame + 1) % MaxFramesInFlight;  // 0 → 1 → 0 → ...
```

---

## Phase 3: Compute Shader (GPU)

### Shader-Ausführung

**Datei**: `raytracing.comp.spv` (kompiliert aus GLSL)

#### Workgroup-Layout

```glsl
layout(local_size_x = 16, local_size_y = 16, local_size_z = 1) in;
```

Jede Workgroup berechnet 16x16 = 256 Pixel parallel.

#### Für jeden Pixel:

1. **Ray Generation**: Strahl von Kamera durch Pixel
2. **Scene Intersection**: Strahl gegen alle Dreiecke testen
3. **Shading**:
    - Beleuchtung berechnen (Phong/Blinn-Phong)
    - Schatten (PCSS - Percentage Closer Soft Shadows)
    - Reflections (Rekursive Rays mit Max Bounces)
4. **Output**: Farbe in Storage Image schreiben

```glsl
imageStore(storageImage, ivec2(pixelX, pixelY), vec4(finalColor, 1.0));
```

---

## Synchronisations-Diagramm

```
Frame 0:
CPU: WaitForFences[0] → AcquireImage → RecordCmd → QueueSubmit → Present → CurrentFrame++
GPU:                     ↓ Signal        Warten      Rendering    ↓ Signal
Semaphore:          imgAvail[0]     →     Wait     renderFinished[imageIdx] → Wait

Frame 1:
CPU: WaitForFences[1] → AcquireImage → RecordCmd → QueueSubmit → Present → CurrentFrame++
GPU:                     ↓ Signal        Warten      Rendering    ↓ Signal
Semaphore:          imgAvail[1]     →     Wait     renderFinished[imageIdx] → Wait
```

**Wichtige Regel**:

- `imageAvailableSemaphores` sind frame-basiert (MaxFramesInFlight = 2)
- `renderFinishedSemaphores` sind image-basiert (SwapchainImageCount = 3)
- Dies verhindert Race Conditions bei der Semaphor-Wiederverwendung

---

## Spezielle Fälle

### Window Resize

**Methode**: `Resize(int width, int height)`

1. `_vk.DeviceWaitIdle()` - Warte auf alle GPU-Operationen
2. Alte Swapchain zerstören
3. Storage Image zerstören
4. Neue Swapchain erstellen (neue Größe)
5. Neues Storage Image erstellen
6. Descriptor Sets aktualisieren

### VSync vs. Non-VSync

**VSync aktiviert (FIFO)**:

- Present wartet auf Monitor-Refresh (z.B. 60 Hz)
- Keine Tearing-Artefakte
- FPS limitiert auf Refresh-Rate

**VSync deaktiviert (IMMEDIATE)**:

- Present sofort nach Rendering
- Maximale FPS möglich
- Potentiell Tearing

---

## Performance-Charakteristiken

### CPU-Aufgaben (leicht)

- Uniform Buffer aktualisieren (~100 Bytes)
- Command Buffer aufzeichnen (~10 Vulkan-Calls)
- Synchronisation (~5 Vulkan-Calls)

### GPU-Aufgaben (schwer)

- Compute Shader: 2560 × 1440 = 3.686.400 Rays
- Pro Ray: Intersections mit allen Dreiecken
- Schatten-Rays: 16-64 Samples pro Pixel
- Reflection-Rays: Rekursiv mit Max Bounces

### Bottleneck

**GPU Compute Performance** ist der limitierende Faktor, nicht CPU oder Vulkan-Overhead.

---

## Fehlerbehandlung

### Swapchain Out-of-Date

```csharp
if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr) {
    return;  // Frame skippen, Resize wird folgen
}
```

### Validation Layers

Bei `EnableValidation = true` prüft Vulkan:

- Korrekte Semaphor-Nutzung
- Memory Leaks
- Pipeline-Fehler
- Synchronisations-Probleme

---

## Zusammenfassung

Die Bildausgabe erfolgt in einem straff getakteten Pipeline:

1. **CPU wartet** auf GPU (Fence)
2. **CPU holt** nächstes Swapchain-Image (Semaphore)
3. **CPU aktualisiert** Szenen-Daten (Uniform Buffers)
4. **CPU zeichnet** Command Buffer auf
5. **CPU submitted** zur GPU-Queue
6. **GPU wartet** auf Image-Verfügbarkeit (Semaphore)
7. **GPU führt** Compute Shader aus (Raytracing)
8. **GPU kopiert** Storage → Swapchain
9. **GPU signalisiert** fertig (Semaphore)
10. **GPU präsentiert** Bild (Present Queue)
11. **Monitor zeigt** Bild an (VSync)

Dieser Zyklus wiederholt sich 60-mal pro Sekunde (bei VSync) und ermöglicht flüssige Echtzeit-Raytracing-Visualisierung.

---

**Dokumentiert am**: 2026-01-29  
**Engine-Version**: Vulkan Compute Raytracing mit PCSS  
**Zielplattform**: Windows mit NVIDIA RTX GPUs
