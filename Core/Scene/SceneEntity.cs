using Core.Scene.Camera;
using Core.Scene.Light;
using Core.Scene.Geometry;

namespace Core.Scene;

public class SceneEntity
{
    public CameraEntity Camera { get; set; }
    public List<LightEntity> Lights { get; }
    public List<TriangleEntity> Triangles { get; }
    public bool UseBVH { get; set; }

    public SceneEntity()
    {
        Camera = new CameraEntity();
        Lights = new List<LightEntity>();
        Triangles = new List<TriangleEntity>();
        UseBVH = false;
    }

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
