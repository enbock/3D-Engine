Resolution Scale (Adaptive GI Resolution)

Beschreibung

Dieses Dokument beschreibt das neue "ResolutionScale"-Feature, das die Sampling-Auflösung für die indirekte
Beleuchtung (GI) reduziert, um Performance zu gewinnen. Die Einstellung wirkt sich auf den Compute-Shader
`pass2b_indirect.comp` aus.

Einstellung

- `ResolutionScale` (int): Skalierungsfaktor für GI-Berechnungen.
    - 1 = native Auflösung (keine Veränderung)
    - 2 = GI wird für jeden 2. Pixel berechnet
    - 4 = GI wird für jeden 4. Pixel berechnet
    - 8, 16 sind ebenfalls möglich (je nach Workgroup-Größe)

Wo wird gesetzt

- Application.RenderSettings -> neue Property `ResolutionScale`.
- Infrastructure.Rendering.Vulkan.Data.RenderSettingsData -> neues Feld `ResolutionScale`.
- VulkanBufferHelper.UpdateSceneBuffers füllt das GPU-UBO-Feld mit dem Wert aus `RenderSettings`.

Shader-Verhalten

- Der Compute-Shader `pass2b_indirect.comp` führt GI-Berechnungen nur für die Sub-Sample-Pixel durch (innerhalb eines
  16x16-Workgroups die Pixel, deren lokale Koordinaten durch `ResolutionScale` teilbar sind).
- Alle anderen Pixel innerhalb derselben 16x16-Workgroup erhalten ihren indirekten Lichtanteil durch bilineare
  Interpolation der vier nächstgelegenen berechneten Sub-Samples (in Shared Memory), wodurch teure BVH-Traces eingespart
  werden.

Wichtige Hinweise und Limitationen

- Die Implementation verwendet 16x16-Workgroups; für bestes Ergebnis sollte `ResolutionScale` ein Teiler von 16 sein (
  z.B. 1,2,4,8,16). Andere Werte funktionieren, führen aber an Workgroup-Rändern zu Gegebenheiten, bei denen die
  Interpolation weniger optimal ist.
- Die Interpolation verwendet nur Werte innerhalb derselben Workgroup. Daher kann es an Kachelgrenzen zu sichtbaren
  Übergängen kommen, besonders bei großen Skalierungen.
- Diese Lösung spart die Anzahl der Trace-Aufrufe erheblich und ist ein guter Performance-Tradeoff für weniger wichtige
  oder mittelgroße Szenen.

Anpassungen/UI

- Um `ResolutionScale` zur Laufzeit steuerbar zu machen, sollte eine UI-Option oder Konfigurationsschnittstelle in der
  Applikation hinzugefügt werden, die `EngineConfig.RenderSettings` aktualisiert.

Weiterführende Schritte

- Optional: Zwei-Pass-Ansatz (Downsample-GI -> Upscale) implementieren, um nahtlose Grenzen zwischen Workgroups zu
  garantieren.
- Optional: Unterstützung dynamischer Workgroup-Größen bzw. spezielle Dispatch-Strategien, um beliebige
  Skalierungsfaktoren stabil zu unterstützen.

Version

- Erstimplementierung: 2026-01-31
