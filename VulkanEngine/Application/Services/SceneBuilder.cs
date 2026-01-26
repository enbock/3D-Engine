using VulkanEngine.Core.Entities;
using VulkanEngine.Core.Math;

namespace VulkanEngine.Application.Services;

public class SceneBuilder
{
    public static Core.Scene CreateDemoScene()
    {
        var scene = new Core.Scene();

        scene.Camera.Position = new Vector3(0, 0, 3);
        scene.Camera.Target = new Vector3(0, 0, 0);

        scene.AddLight(Light.CreateDirectional(
            new Vector3(0.5f, 0.7f, 1.0f),
            Color.White,
            1.5f
        ));

        scene.AddLight(Light.CreateAmbient(
            Color.White,
            0.3f
        ));

        scene.AddTriangle(new Triangle(
            new Vector3(-2, -2, 0),
            new Vector3(2, -2, 0),
            new Vector3(0, 2, 0),
            Color.Red
        ));

        CreateCubeTriangles(new Vector3(-2, 0, 0), new Vector3(1, 1, 1), Color.Red, scene);
        CreateCubeTriangles(new Vector3(0, 0, 0), new Vector3(0.8f, 1, 0.8f), Color.Green, scene);
        CreateCubeTriangles(new Vector3(2, 0, 0), new Vector3(1.2f, 1.2f, 1.2f), Color.Blue, scene);

        CreateFloorPlane(new Vector3(0, -1, 0), 20, new Color(0.6f, 0.6f, 0.6f), scene);

        return scene;
    }

    private static void CreateCubeTriangles(Vector3 center, Vector3 size, Color color, Core.Scene scene)
    {
        var halfSize = size / 2f;

        var v0 = center + new Vector3(-halfSize.X, -halfSize.Y, -halfSize.Z);
        var v1 = center + new Vector3(halfSize.X, -halfSize.Y, -halfSize.Z);
        var v2 = center + new Vector3(halfSize.X, halfSize.Y, -halfSize.Z);
        var v3 = center + new Vector3(-halfSize.X, halfSize.Y, -halfSize.Z);
        var v4 = center + new Vector3(-halfSize.X, -halfSize.Y, halfSize.Z);
        var v5 = center + new Vector3(halfSize.X, -halfSize.Y, halfSize.Z);
        var v6 = center + new Vector3(halfSize.X, halfSize.Y, halfSize.Z);
        var v7 = center + new Vector3(-halfSize.X, halfSize.Y, halfSize.Z);

        scene.AddTriangle(new Triangle(v0, v1, v2, color));
        scene.AddTriangle(new Triangle(v0, v2, v3, color));

        scene.AddTriangle(new Triangle(v5, v4, v7, color));
        scene.AddTriangle(new Triangle(v5, v7, v6, color));

        scene.AddTriangle(new Triangle(v4, v0, v3, color));
        scene.AddTriangle(new Triangle(v4, v3, v7, color));

        scene.AddTriangle(new Triangle(v1, v5, v6, color));
        scene.AddTriangle(new Triangle(v1, v6, v2, color));

        scene.AddTriangle(new Triangle(v3, v2, v6, color));
        scene.AddTriangle(new Triangle(v3, v6, v7, color));

        scene.AddTriangle(new Triangle(v4, v5, v1, color));
        scene.AddTriangle(new Triangle(v4, v1, v0, color));
    }

    private static void CreateFloorPlane(Vector3 center, float size, Color color, Core.Scene scene)
    {
        var halfSize = size / 2f;

        var v0 = center + new Vector3(-halfSize, 0, -halfSize);
        var v1 = center + new Vector3(halfSize, 0, -halfSize);
        var v2 = center + new Vector3(halfSize, 0, halfSize);
        var v3 = center + new Vector3(-halfSize, 0, halfSize);

        scene.AddTriangle(new Triangle(v0, v1, v2, color));
        scene.AddTriangle(new Triangle(v0, v2, v3, color));
    }
}
