using Core.Math;
using Core.Scene.Geometry;
using Infrastructure.Assets;

namespace Core.Assets;

public interface ModelLoader
{
    ModelData LoadModel(string filePath);
}

public class ModelData
{
    public List<MeshData> Meshes { get; } = [];
    public string Name { get; init; } = string.Empty;
}

public record MeshData
{
    public List<VertexData> Vertices { get; } = [];
    public List<int> Indices { get; } = [];
    public MaterialData Material { get; set; } = new();
    public string Name { get; init; } = string.Empty;
}

public class VertexData
{
    public Vector3 Position { get; init; }
    public Vector3 Normal { get; init; }
    public TextureCoordinate UV { get; init; }
}

public record MaterialData
{
    public Color BaseColor { get; set; } = new(1.0f, 1.0f, 1.0f);
    public TextureHandle? BaseColorTexture { get; set; }
    public TextureHandle? NormalTexture { get; set; }
    public float Metallic { get; set; }
    public float Roughness { get; set; } = 1.0f;
    public float Transparency { get; set; }
    public float IndexOfRefraction { get; set; } = 1.5f;
    public string Name { get; init; } = string.Empty;
}