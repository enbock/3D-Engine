# Caustics Option

Dieses Dokument beschreibt die neue Option `EnableCaustics` in `RenderSettings`.

Zweck

- Ermöglicht das Ein- bzw. Ausschalten der Caustics-Berechnung.

Änderungen

- In `Application/RenderSettings.cs` wurde das Property `EnableCaustics` ergänzt.
- In den Presets ist `EnableCaustics` nun explizit gesetzt. `UltraPerformance` hat `EnableCaustics = false`.

Hinweis

- Falls Shader oder native Strukturen (Uniform-Buffer) Caustics-Flags erwarten, muss `RenderSettingsData` in
  `Infrastructure/Rendering/Vulkan/Data/UniformDataStructures.cs` ggf. angepasst und die Serialisierung in den
  Renderer-Tasks aktualisiert.
