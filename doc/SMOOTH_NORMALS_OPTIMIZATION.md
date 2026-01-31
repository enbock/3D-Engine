# Smooth Normals Optimization in ModelLoader

## Overview

The `CalculateSmoothNormals` method in the `ModelLoader` class was optimized to address performance issues and ensure a
clean rendering of models, particularly the teapot model. The previous implementation had a computational complexity of
O(n²), which caused the program to freeze when processing models with a large number of triangles.

## Changes Made

### 1. Smoothing-Angle-Threshold Adjustment

The smoothing angle threshold was adjusted to **0.9475f**. This value corresponds to a maximum angle of approximately *
*18°** between face normals for smoothing to occur. This ensures that:

- Smooth shading is applied to surfaces with small angles between face normals.
- Sharp edges, such as at the base of the teapot handle and spout, are preserved.

### 2. Performance Optimization

The `CalculateSmoothNormals` method was optimized by introducing a **HashMap for position lookups**. This significantly
reduced the computational complexity from O(n²) to O(n), making the method scalable for models with a large number of
triangles.

#### Key Changes:

- **HashMap (`positionToTriangles`)**: A dictionary was added to map vertex positions to the list of triangle indices
  that share the same position.
- **Two-Phase Process**:
    1. **Indexing Phase**: All vertex positions are indexed in the `positionToTriangles` dictionary.
    2. **Lookup Phase**: Neighboring triangles are efficiently retrieved using the dictionary, avoiding the need for
       nested loops.

### 3. Removal of Automatic Normal Flipping in Shaders

The automatic normal flipping logic in the shaders was removed. This logic was causing issues with incorrect normal
directions at concave areas, leading to visual artifacts. By relying solely on the normals calculated in the
`ModelLoader`, the rendering now produces accurate results.

## Results

- The teapot model now renders correctly with smooth shading and sharp edges at appropriate locations.
- The performance of the `CalculateSmoothNormals` method has been significantly improved, allowing the program to handle
  complex models without freezing.

## Future Considerations

- Further testing with other models to ensure the robustness of the solution.
- Investigate additional optimizations for extremely large models.

## Code Reference

The updated `CalculateSmoothNormals` method is located in the `ModelLoader` class within the
`Infrastructure/Assets/ModelLoader.cs` file.
