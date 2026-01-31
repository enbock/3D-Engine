using Core.Assets;
using Core.Scene.Camera;
using Core.Scene.Geometry;
using Core.Scene.Light;

namespace Core.Scene;

public class SceneEntity
{
    public CameraEntity Camera { get; set; } = new();
    public List<LightEntity> Lights { get; } = [];
    public List<TriangleEntity> Triangles { get; } = [];
    public List<int> TextureIds { get; } = [];

    public void AddLight(LightEntity light)
    {
        Lights.Add(light);
    }

    public void AddTriangle(TriangleEntity triangle)
    {
        Triangles.Add(triangle);
    }

    public void AddModel(ModelData modelData)
    {
        foreach (MeshData mesh in modelData.Meshes)
        {
            AddMesh(mesh);
        }
    }

    public void AddMesh(MeshData mesh)
    {
        MaterialEntity material = new(mesh.Material.BaseColor, mesh.Material.Transparency, mesh.Material.IndexOfRefraction)
        {
            Metallic = mesh.Material.Metallic,
            Roughness = mesh.Material.Roughness,
            BaseColorTextureId = mesh.Material.BaseColorTexture?.Id ?? -1,
            NormalTextureId = mesh.Material.NormalTexture?.Id ?? -1
        };

        if (material.HasBaseColorTexture && !TextureIds.Contains(material.BaseColorTextureId))
            TextureIds.Add(material.BaseColorTextureId);
        if (material.HasNormalTexture && !TextureIds.Contains(material.NormalTextureId))
            TextureIds.Add(material.NormalTextureId);

        for (int i = 0; i < mesh.Indices.Count; i += 3)
        {
            VertexData v0 = mesh.Vertices[mesh.Indices[i]];
            VertexData v1 = mesh.Vertices[mesh.Indices[i + 1]];
            VertexData v2 = mesh.Vertices[mesh.Indices[i + 2]];

            TriangleEntity triangle = new(
                v0.Position, v1.Position, v2.Position,
                material,
                v0.Normal, v1.Normal, v2.Normal,
                v0.UV, v1.UV, v2.UV);

            Triangles.Add(triangle);
        }
    }

    public void Clear()
    {
        Lights.Clear();
        Triangles.Clear();
        TextureIds.Clear();
    }
}