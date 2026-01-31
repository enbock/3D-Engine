using System.Numerics;
using Core.Assets;
using Core.Math;
using Core.Scene.Geometry;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using GltfTexture = SharpGLTF.Schema2.Texture;
using Vector3 = System.Numerics.Vector3;

namespace Infrastructure.Assets;

public class ModelLoader : IModelLoader
{
    private readonly string _basePath;
    private readonly TextureLoader _textureLoader;

    public ModelLoader(TextureLoader textureLoader, string basePath = "")
    {
        _textureLoader = textureLoader;
        _basePath = basePath;
    }

    public ModelData LoadModel(string filePath)
    {
        string fullPath = string.IsNullOrEmpty(_basePath) ? filePath : Path.Combine(_basePath, filePath);

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
                MeshData meshData = ExtractMeshData(primitive, worldTransform, model, fullPath);
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

    private MeshData ExtractMeshData(MeshPrimitive primitive, Matrix4x4 transform, ModelRoot model, string modelPath)
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
            Vector3 norm = i < normals.Count
                ? Vector3.TransformNormal(normals[i], transform)
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

        meshData.Material = ExtractMaterial(primitive.Material, model, modelPath);

        return meshData;
    }

    private MaterialData ExtractMaterial(Material? gltfMaterial, ModelRoot model, string modelPath)
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
                return _textureLoader.LoadTexture(texturePath);
            }

            return null;
        }

        byte[] imageData = content.Content.ToArray();
        string textureName = image.Name ?? $"texture_{texture.LogicalIndex}";

        return isSrgb
            ? _textureLoader.LoadTextureFromBytes(imageData, textureName)
            : _textureLoader.LoadNormalMapFromBytes(imageData, textureName);
    }
}