using VulkanEngine.Core.Entities;

namespace VulkanEngine.Core;

public class Scene
{
    public Camera Camera { get; set; }
    public List<Light> Lights { get; }
    public List<Triangle> Triangles { get; }

    public Scene(Camera? camera = null)
    {
        Camera = camera ?? new Camera();
        Lights = new List<Light>();
        Triangles = new List<Triangle>();
    }

    public void AddLight(Light light)
    {
        Lights.Add(light);
    }

    public void AddTriangle(Triangle triangle)
    {
        Triangles.Add(triangle);
    }

    public void AddTriangles(IEnumerable<Triangle> triangles)
    {
        Triangles.AddRange(triangles);
    }

    public void Clear()
    {
        Lights.Clear();
        Triangles.Clear();
    }
}
