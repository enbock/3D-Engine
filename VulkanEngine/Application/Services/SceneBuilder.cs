using VulkanEngine.Core.Entities;
using VulkanEngine.Core.Math;

namespace VulkanEngine.Application.Services;

public class SceneBuilder
{
    public static Core.Scene CreateDemoScene()
    {
        return CreateSimpleScene();
    }

    public static Core.Scene CreateSimpleScene()
    {
        var scene = new Core.Scene();

        scene.Camera.Position = new Vector3(0, 3, 10);
        scene.Camera.Target = new Vector3(0, 1, 0);

        scene.AddLight(Light.CreateAmbient(Color.White, 0.3f));
        scene.AddLight(Light.CreateDirectional(new Vector3(0.3f, -0.7f, -0.5f), Color.White, 1.0f));

        scene.AddTriangle(new Triangle(
            new Vector3(-2, 0, -1),
            new Vector3(-1, 0, -1),
            new Vector3(-1.5f, 2, -1),
            Color.Red
        ));

        scene.AddTriangle(new Triangle(
            new Vector3(-0.3f, 0, 0),
            new Vector3(0.7f, 0, 0),
            new Vector3(0.2f, 2, 0),
            Color.Green
        ));

        scene.AddTriangle(new Triangle(
            new Vector3(1.2f, 0, 1),
            new Vector3(2.2f, 0, 1),
            new Vector3(1.7f, 2, 1),
            Color.Blue
        ));

        CreateFloorPlane(new Vector3(0, 0, 0), 20, new Color(0.5f, 0.5f, 0.5f), scene);

        scene.BuildBVH();

        return scene;
    }

    public static Core.Scene CreateComplexScene()
    {
        var scene = new Core.Scene();

        scene.Camera.Position = new Vector3(0, 5, 15);
        scene.Camera.Target = new Vector3(0, 0, 0);

        scene.AddLight(Light.CreateDirectional(new Vector3(0.3f, -0.7f, -0.5f), Color.White, 0.8f));
        scene.AddLight(Light.CreateAmbient(Color.White, 0.2f));
        scene.AddLight(Light.CreatePoint(new Vector3(-5, 3, 0), new Color(1, 0.3f, 0.3f), 2.0f));
        scene.AddLight(Light.CreatePoint(new Vector3(5, 3, 0), new Color(0.3f, 0.3f, 1), 2.0f));

        var random = new Random(42);
        for (int i = 0; i < 20; i++)
        {
            var x = (float)(random.NextDouble() * 16 - 8);
            var y = (float)(random.NextDouble() * 4);
            var z = (float)(random.NextDouble() * 16 - 8);
            var size = (float)(random.NextDouble() * 0.5 + 0.3);

            var color = new Color(
                (float)random.NextDouble(),
                (float)random.NextDouble(),
                (float)random.NextDouble()
            );

            CreateCubeTriangles(
                new Vector3(x, y, z),
                new Vector3(size, size, size),
                color,
                scene
            );
        }

        CreateFloorPlane(new Vector3(0, -1, 0), 40, new Color(0.3f, 0.3f, 0.35f), scene);

        scene.BuildBVH();

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

        scene.AddTriangle(new Triangle(v0, v2, v1, color));
        scene.AddTriangle(new Triangle(v0, v3, v2, color));
    }
}
