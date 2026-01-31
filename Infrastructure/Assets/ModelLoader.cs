using System.Numerics;
using Core.Assets;
using Core.Math;
using Core.Scene.Geometry;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using GltfTexture = SharpGLTF.Schema2.Texture;
using Vector3 = System.Numerics.Vector3;

namespace Infrastructure.Assets;

public class ModelLoader(TextureLoader textureLoader, string basePath = "", bool calculateNormals = true) : Core.Assets.ModelLoader
{
    private readonly bool _calculateNormals = calculateNormals;

    public ModelData LoadModel(string filePath)
    {
        string fullPath = string.IsNullOrEmpty(basePath) ? filePath : Path.Combine(basePath, filePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Model file not found: {fullPath}");

        ModelRoot model = ModelRoot.Load(fullPath);
        string modelName = Path.GetFileNameWithoutExtension(filePath);

        ModelData modelData = new()
        {
            Name = modelName
        };

        foreach (Node node in model.LogicalNodes)
        {
            if (node.Mesh == null) continue;

            Matrix4x4 worldTransform = node.WorldMatrix;

            foreach (MeshPrimitive primitive in node.Mesh.Primitives)
            {
                MeshData meshData = ExtractMeshData(primitive, worldTransform, fullPath);
                meshData = meshData with
                {
                    Name = $"{node.Name ?? modelName}_{modelData.Meshes.Count}"
                };
                modelData.Meshes.Add(meshData);
            }
        }

        Console.WriteLine($"Model loaded: {modelName} with {modelData.Meshes.Count} meshes");
        return modelData;
    }

    private MeshData ExtractMeshData(MeshPrimitive primitive, Matrix4x4 transform, string modelPath)
    {
        MeshData meshData = new();

        IList<Vector3> positions = primitive.GetVertexAccessor("POSITION")?.AsVector3Array()
                                   ?? Array.Empty<Vector3>();
        IList<Vector3> normals = primitive.GetVertexAccessor("NORMAL")?.AsVector3Array()
                                 ?? Array.Empty<Vector3>();
        IList<Vector2> texCoords = primitive.GetVertexAccessor("TEXCOORD_0")?.AsVector2Array()
                                   ?? Array.Empty<Vector2>();

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = Vector3.Transform(positions[i], transform);
            Vector3 norm = !_calculateNormals && i < normals.Count
                ? normals[i]
                : Vector3.UnitY;
            Vector2 uv = i < texCoords.Count ? texCoords[i] : Vector2.Zero;

            meshData.Vertices.Add(new VertexData
            {
                Position = new Core.Math.Vector3(pos.X, pos.Y, pos.Z),
                Normal = new Core.Math.Vector3(norm.X, norm.Y, norm.Z).Normalized,
                UV = new TextureCoordinate(uv.X, uv.Y)
            });
        }

        IEnumerable<(int, int, int)> triangles = primitive.GetTriangleIndices();
        foreach ((int a, int b, int c) in triangles)
        {
            meshData.Indices.Add(a);
            meshData.Indices.Add(b);
            meshData.Indices.Add(c);
        }

        if (_calculateNormals)
        {
            // CalculateFaceNormals(meshData);
            CalculateSmoothNormals(meshData);
        }

        meshData.Material = ExtractMaterial(primitive.Material, modelPath);

        return meshData;
    }

    private void CalculateFaceNormals(MeshData meshData)
    {
        for (int i = 0; i < meshData.Indices.Count; i += 3)
        {
            int idx0 = meshData.Indices[i];
            int idx1 = meshData.Indices[i + 1];
            int idx2 = meshData.Indices[i + 2];

            Core.Math.Vector3 p0 = meshData.Vertices[idx0].Position;
            Core.Math.Vector3 p1 = meshData.Vertices[idx1].Position;
            Core.Math.Vector3 p2 = meshData.Vertices[idx2].Position;

            Core.Math.Vector3 edge1 = p1 - p0;
            Core.Math.Vector3 edge2 = p2 - p0;
            Core.Math.Vector3 faceNormal = Core.Math.Vector3.Cross(edge1, edge2).Normalized;

            VertexData v0 = meshData.Vertices[idx0];
            VertexData v1 = meshData.Vertices[idx1];
            VertexData v2 = meshData.Vertices[idx2];

            meshData.Vertices[idx0] = new VertexData
            {
                Position = v0.Position,
                Normal = faceNormal,
                UV = v0.UV
            };
            meshData.Vertices[idx1] = new VertexData
            {
                Position = v1.Position,
                Normal = faceNormal,
                UV = v1.UV
            };
            meshData.Vertices[idx2] = new VertexData
            {
                Position = v2.Position,
                Normal = faceNormal,
                UV = v2.UV
            };
        }
    }

    private void CalculateSmoothNormals(MeshData meshData)
    {
        const float smoothingAngleThreshold = 0.9475f;

        List<VertexData> originalVertices = new(meshData.Vertices);
        List<int> originalIndices = new(meshData.Indices);

        Dictionary<int, Core.Math.Vector3> faceNormals = new();
        Dictionary<string, List<int>> positionToTriangles = new();

        for (int triIdx = 0; triIdx < originalIndices.Count / 3; triIdx++)
        {
            int i = triIdx * 3;
            Core.Math.Vector3 p0 = originalVertices[originalIndices[i]].Position;
            Core.Math.Vector3 p1 = originalVertices[originalIndices[i + 1]].Position;
            Core.Math.Vector3 p2 = originalVertices[originalIndices[i + 2]].Position;

            Core.Math.Vector3 edge1 = p1 - p0;
            Core.Math.Vector3 edge2 = p2 - p0;
            faceNormals[triIdx] = Core.Math.Vector3.Cross(edge1, edge2).Normalized;

            string key0 = GetPositionKey(p0);
            string key1 = GetPositionKey(p1);
            string key2 = GetPositionKey(p2);

            if (!positionToTriangles.ContainsKey(key0)) positionToTriangles[key0] = [];
            if (!positionToTriangles.ContainsKey(key1)) positionToTriangles[key1] = [];
            if (!positionToTriangles.ContainsKey(key2)) positionToTriangles[key2] = [];

            positionToTriangles[key0].Add(triIdx);
            positionToTriangles[key1].Add(triIdx);
            positionToTriangles[key2].Add(triIdx);
        }

        meshData.Vertices.Clear();
        meshData.Indices.Clear();

        for (int triIdx = 0; triIdx < originalIndices.Count / 3; triIdx++)
        {
            int i = triIdx * 3;
            Core.Math.Vector3 currentFaceNormal = faceNormals[triIdx];

            for (int v = 0; v < 3; v++)
            {
                VertexData vertex = originalVertices[originalIndices[i + v]];
                Core.Math.Vector3 pos = vertex.Position;
                string posKey = GetPositionKey(pos);

                Core.Math.Vector3 smoothNormal = currentFaceNormal;

                if (positionToTriangles.TryGetValue(posKey, out List<int>? neighborTris))
                {
                    foreach (int neighborTriIdx in neighborTris)
                    {
                        if (neighborTriIdx == triIdx) continue;

                        Core.Math.Vector3 otherFaceNormal = faceNormals[neighborTriIdx];
                        float dot = Core.Math.Vector3.Dot(currentFaceNormal, otherFaceNormal);

                        if (dot > smoothingAngleThreshold)
                        {
                            smoothNormal = smoothNormal + otherFaceNormal;
                        }
                    }
                }

                smoothNormal = smoothNormal.Normalized;

                meshData.Vertices.Add(new VertexData
                {
                    Position = pos,
                    Normal = smoothNormal,
                    UV = vertex.UV
                });
            }

            int newIdx = triIdx * 3;
            meshData.Indices.Add(newIdx);
            meshData.Indices.Add(newIdx + 1);
            meshData.Indices.Add(newIdx + 2);
        }
    }

    private static string GetPositionKey(Core.Math.Vector3 pos)
    {
        int x = (int)(pos.X * 10000);
        int y = (int)(pos.Y * 10000);
        int z = (int)(pos.Z * 10000);
        return $"{x}_{y}_{z}";
    }

    private Core.Math.Vector3 CalculateAverageNormal(List<Core.Math.Vector3> normals, Core.Math.Vector3 faceNormal)
    {
        Core.Math.Vector3 avg = new(0, 0, 0);
        foreach (Core.Math.Vector3 n in normals)
        {
            avg = avg + n;
        }

        avg = avg.Normalized;

        if (Core.Math.Vector3.Dot(avg, faceNormal) < 0)
        {
            avg = new Core.Math.Vector3(-avg.X, -avg.Y, -avg.Z);
        }

        return avg;
    }

    private MaterialData ExtractMaterial(Material? gltfMaterial, string modelPath)
    {
        MaterialData material = new();

        if (gltfMaterial == null)
            return material;

        material = material with
        {
            Name = gltfMaterial.Name ?? "Default"
        };

        MaterialChannel? baseColorChannel = gltfMaterial.FindChannel("BaseColor");
        if (baseColorChannel.HasValue)
        {
            Vector4 color = baseColorChannel.Value.Color;
            material.BaseColor = new Color(color.X, color.Y, color.Z);
            material.Transparency = 1.0f - color.W;

            GltfTexture? texture = baseColorChannel.Value.Texture;
            if (texture != null)
            {
                material.BaseColorTexture = LoadGltfTexture(texture, modelPath, true);
            }
        }

        MaterialChannel? metallicRoughnessChannel = gltfMaterial.FindChannel("MetallicRoughness");
        if (metallicRoughnessChannel.HasValue)
        {
            material.Metallic = metallicRoughnessChannel.Value.GetFactor("MetallicFactor");
            material.Roughness = metallicRoughnessChannel.Value.GetFactor("RoughnessFactor");
        }

        MaterialChannel? normalChannel = gltfMaterial.FindChannel("Normal");
        if (normalChannel.HasValue)
        {
            GltfTexture? normalTexture = normalChannel.Value.Texture;
            if (normalTexture != null)
            {
                material.NormalTexture = LoadGltfTexture(normalTexture, modelPath, false);
            }
        }

        return material;
    }

    private TextureHandle? LoadGltfTexture(GltfTexture texture, string modelPath, bool isSrgb)
    {
        Image? image = texture.PrimaryImage;
        if (image == null) return null;

        MemoryImage content = image.Content;

        if (content.IsEmpty)
        {
            string? sourceUri = image.Content.SourcePath;
            if (!string.IsNullOrEmpty(sourceUri))
            {
                string texturePath = Path.Combine(Path.GetDirectoryName(modelPath) ?? "", sourceUri);
                return textureLoader.LoadTexture(texturePath);
            }

            return null;
        }

        byte[] imageData = content.Content.ToArray();
        string textureName = image.Name ?? $"texture_{texture.LogicalIndex}";

        return isSrgb
            ? textureLoader.LoadTextureFromBytes(imageData, textureName)
            : textureLoader.LoadNormalMapFromBytes(imageData, textureName);
    }
}