# Modell‑ und Texture‑Laden — Empfehlung und Umsetzungsplan

> **Status: IMPLEMENTIERT** ✅ (2026-01-31)
> Siehe [MODEL_TEXTURE_LOADING_IMPL.md](./MODEL_TEXTURE_LOADING_IMPL.md) für Details.

Ziel: glTF/GLB als primäres Laufzeitformat unterstützen und gleichzeitig grundlegenden Textur‑Support (BaseColor,
Normal) implementieren.

Kurzempfehlung

- Ja — Texturen sollten zusammen mit dem Model‑Loader implementiert werden. glTF ist für PBR/Materialdaten ausgelegt;
  ohne Texture‑Support wirken Modelle blass oder einfarbig. Außerdem erfordert die Vulkan‑Ressourcenverwaltung (Images,
  Sampler, DescriptorSets) Änderungen, die sinnvollerweise gleich beim Model‑Loader berücksichtigt werden.

Priorisierte Minimal‑Featureliste (sofort implementieren)

1. Image‑Decoding
    - Formate: PNG, JPEG (erstes Ziel).
    - Bibliothek: SixLabors.ImageSharp oder StbImageSharp (C#).
2. Texture GPU‑Upload
    - Staging Buffer → VkImage, Layout‑Transitions, einfache Mipmap‑Erzeugung (optional CPU/GPU).
3. Sampler
    - Basissampler: linear filtering, adress mode repeat/clamp, optionale Anisotropie.
4. Material → Texture Mapping
    - Unterstützung für BaseColor/Albedo Map und Normal Map.
    - Material‑Struktur enthält Referenzen auf Texture‑Handles (GPU‑Ressource), nicht rohe Bytes.
5. Shader‑Support
    - Einfacher PBR bzw. alternativer Lambert Branch: BaseColor + Normal.
6. Asset API
    - ModelLoader liefert Meshes + Materials; Material enthält Textur‑Handles.
7. Tests & Assets
    - Testassets: ein kleines glb mit BaseColor; optional OBJ+MTL mit Diffuse‑Texture.

Erweiterte Features (später)

- Vollständige glTF PBR (metallicRoughness, occlusion, emissive).
- Komprimierte Texturen (KTX2 / BCn), Texture Streaming, Texture Atlasing.
- Animation/Skinning (falls benötigt): glTF unterstützt Animationen; dafür ist zusätzliche Bindung an Skeletal‑System
  und GPU‑Buffers nötig.

Wichtige technische Hinweise (Vulkan / C#)

- Farb‑Raum: BaseColor‑Maps sollten als sRGB hochgeladen werden (VK_FORMAT_R8G8B8A8_SRGB), Normal‑Maps in linear.
- Mipmap: Für gute Filterqualität Mips nutzen; Mips können auf GPU per Blit erzeugt werden.
- Sampler: VK_FILTER_LINEAR, VK_SAMPLER_ADDRESS_MODE_REPEAT oder CLAMP_TO_EDGE; anisotrope Filterung aktiv abfragen.
- DescriptorSets: Pro Material ein DescriptorSet mit combined image sampler oder später Descriptor Indexing/Arrays.
- Image Upload: staging buffer (HOST_VISIBLE) -> copyBufferToImage -> image layout transition to
  SHADER_READ_ONLY_OPTIMAL.
- Kanalanzahl: verschiedene Source‑Formate (RGB/RGBA) auf RGBA auffüllen.
- Cleanup: Dispose/Free für VkImage, ImageView, DeviceMemory und Sampler implementieren.

Empfohlene Bibliotheken & Tools

- glTF Parser: SharpGLTF (robust, .gltf/.glb, extrahiert Materialien & Bildpfade).
- Image Decoding: SixLabors.ImageSharp (umfangreich) oder StbImageSharp (klein, performant).
- Vulkan Bindings: Verwende die Binding‑Lib, die das Projekt bereits nutzt (z. B. Silk.NET.Vulkan, Vortice.Vulkan,
  VulkanSharp).
- Optional: FBX‑/Konverter: FBX2glTF oder Assimp/AssimpNet für Offline‑Konvertierung.

Konkrete Integrationspunkte im Projekt (Vorschlag)

- Neue Domain‑Typen:
    - `Core/Geometry/Material.cs` — Material‑Domain (BaseColorTextureId, NormalTextureId, Faktoren).
- Infrastruktur:
    - `Infrastructure/Assets/Texture.cs` — kapselt VkImage, VkImageView, VkDeviceMemory, VkSampler + Dispose.
    - `Infrastructure/Assets/TextureLoader.cs` — dekodiert Bilddateien, lädt Textur in Vulkan, gibt
      Texture‑Objekt/Handle zurück.
    - `Infrastructure/Assets/ModelLoader.cs` — benutzt SharpGLTF, erstellt Mesh‑ und Material‑Instanzen und ruft
      `TextureLoader` für jede Referenz auf.
- DI / ServiceContainer:
    - `Container/ServiceContainer.cs` — registriert `ITextureLoader`, `IModelLoader`.
- Core/Scene & Mesh:
    - `Core/Geometry/Mesh.cs` um Material‑Referenz erweitern (statt nur Farbe).
    - `Application/Scene/SceneBuilder` Beispiel: `scene.AddMeshFromFile("assets/models/xxx.glb")`.

Tests & Testassets

- Lege `assets/tests/` an mit:
    - `test_basecolor.glb` (BaseColor Texture)
    - `test_obj_mtl.zip` (OBJ + MTL + Texture)
- Tests:
    - Loader gibt Meshe zurück (>0).
    - Material referenziert TextureHandle (nicht null).
    - Texture geladen → ImageView/Sampler existieren, Größe entspricht dekodiertem Bild.
    - Render‑Smoke Test: Szene mit Textur rendert sichtbar anderes Ergebnis als einfarbiges Material.

Edge‑Cases & Risiken

- Eingebettete Bilder (data URI) vs. externe Pfade behandeln.
- Fehlende Dateien / inkonsistente Materialien → Fallback‑Textur (magenta).
- Unterschiedliche Farbräume beachten (sRGB vs linear).
- Große Texturen / OOM → späteres Streaming/LOD nötig.
- Async‑Laden: Texturen in Hintergrund laden und Platzhalter‑Texture verwenden.

## Smooth Normals Optimization

### Hintergrund

Die Berechnung von glatten Normalen (Smooth Normals) ist ein wesentlicher Bestandteil des Model Loadings, um eine
realistische Darstellung von 3D-Modellen zu gewährleisten. Dabei werden die Normalen von benachbarten Flächen gemittelt,
um weiche Übergänge zwischen den Flächen zu erzeugen.

### Optimierung

Die Methode `CalculateSmoothNormals` wurde optimiert, um die Performance zu verbessern und visuelle Artefakte zu
vermeiden. Die wichtigsten Änderungen sind:

1. **Smoothing-Angle-Threshold**: Der Schwellenwert wurde auf **0.9475f** gesetzt, was einem maximalen Winkel von ca. *
   *18°** zwischen den Flächennormalen entspricht. Dies sorgt für eine ausgewogene Glättung, bei der scharfe Kanten
   erhalten bleiben.

2. **HashMap für Position-Lookups**: Eine HashMap (`positionToTriangles`) wurde eingeführt, um die Dreiecke, die sich
   eine Position teilen, effizient zu finden. Dies reduziert die Komplexität der Methode von O(n²) auf O(n).

3. **Entfernung des automatischen Normalen-Flippens**: Die automatische Umkehrung der Normalen im Shader wurde entfernt,
   da sie zu falschen Darstellungen an konkaven Bereichen führte. Stattdessen werden die Normalen direkt aus der
   `ModelLoader`-Berechnung verwendet.

### Ergebnisse

- Die Performance der Methode wurde erheblich verbessert, sodass auch komplexe Modelle wie der Teapot ohne Einfrieren
  geladen werden können.
- Die Darstellung des Teapots ist nun korrekt, mit glatten Oberflächen und scharfen Kanten an den richtigen Stellen.

### Code-Referenz

Die optimierte Methode `CalculateSmoothNormals` befindet sich in der Klasse `ModelLoader` im Verzeichnis
`Infrastructure/Assets/ModelLoader.cs`.

Nächste Schritte (Optionen zum Start)

A) ✅ **IMPLEMENTIERT** - Minimale TextureLoader + ModelLoader‑Integration (C# Vulkan):

- StbImageSharp für Decoding, Vulkan Upload, Sampler, Descriptor Registration
- UV-Koordinaten und TextureIds in TriangleData
- Shader‑Anpassung für BaseColor+Normal steht noch aus

B) volle metallicRoughness PBR.

**Gewählte Image‑Lib**: StbImageSharp (MIT-Lizenz, klein und performant)
