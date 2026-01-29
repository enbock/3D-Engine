using Core.Scene.Camera;
using Core.Scene.Geometry;
using Core.Scene.Light;

namespace Core.Scene;

public class SceneEntity
{
    public SceneEntity()
    {
        Camera = new CameraEntity();
        Lights = new List<LightEntity>();
        Triangles = new List<TriangleEntity>();
        UseBVH = false;
    }

    public CameraEntity Camera { get; set; }
    public List<LightEntity> Lights { get; }
    public List<TriangleEntity> Triangles { get; }
    public bool UseBVH { get; set; }

    public void AddLight(LightEntity light)
    {
        Lights.Add(light);
    }

    public void AddTriangle(TriangleEntity triangle)
    {
        Triangles.Add(triangle);
    }

    public void Clear()
    {
        Lights.Clear();
        Triangles.Clear();
    }
}