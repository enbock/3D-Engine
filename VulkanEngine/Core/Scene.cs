using VulkanEngine.Core.Entities;
using VulkanEngine.Core.Acceleration;

namespace VulkanEngine.Core;

public class Scene
{
    public Camera Camera { get; set; }
    public List<Light> Lights { get; }
    public List<Triangle> Triangles { get; }
    public BVHNode? BVH { get; private set; }
    public bool UseBVH { get; set; }

    public Scene(Camera? camera = null)
    {
        Camera = camera ?? new Camera();
        Lights = new List<Light>();
        Triangles = new List<Triangle>();
        UseBVH = false;
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

    public void BuildBVH()
    {
        if (Triangles.Count > 0)
        {
            Console.WriteLine($"Building BVH for {Triangles.Count} triangles...");
            var startTime = System.Diagnostics.Stopwatch.StartNew();
            BVH = BVHBuilder.Build(Triangles);
            startTime.Stop();
            Console.WriteLine($"BVH built in {startTime.ElapsedMilliseconds}ms");
            UseBVH = true;
        }
    }

    public void Clear()
    {
        Lights.Clear();
        Triangles.Clear();
        BVH = null;
        UseBVH = false;
    }
}
