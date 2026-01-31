using Application.Scene;
using Core.Assets;
using Core.Math;
using Core.Scene.Geometry;
using Core.Scene.Light;
using Infrastructure.Assets;

namespace Core.Scene;

public static class SceneBuilderService
{
    public static void CreateSimpleScene(SceneEntity scene)
    {
        scene.AddLight(LightEntity.CreateDirectional(new Vector3(0.5f, -1.0f, 0.5f), Color.White, 0.5f, 0.1f));
        scene.AddLight(LightEntity.CreatePoint(new Vector3(-3, 4, 2), new Color(1.0f, 0.9f, 0.8f), 1.0f, 0.005f, 0.25f));

        GeometryGenerator.AddCylinder(scene, new Vector3(-2, 1, 0), 0.5f, 2.0f, 16, new Color(1.0f, 0.0f, 0.0f), ShadingMode.HalfSmooth);
        GeometryGenerator.AddSphere(scene, new Vector3(0, 1, 0), 0.8f, 12, 16, new Color(0.0f, 1.0f, 0.0f), ShadingMode.HalfSmooth);
        GeometryGenerator.AddCube(scene, new Vector3(2, 1, 0), 1.5f, new Color(0.0f, 0.0f, 1.0f));

        GeometryGenerator.AddGlassSphere(scene, new Vector3(-1, 0.6f, 2), 0.6f, 16, 24, new Color(0.95f, 0.98f, 1.0f));
        GeometryGenerator.AddDiamondSphere(scene, new Vector3(1.5f, 0.5f, 2.5f), 0.5f, 20, 32, new Color(1.0f, 1.0f, 1.0f));
        GeometryGenerator.AddWaterSphere(scene, new Vector3(0.2f, 0.4f, 1.5f), 0.4f, 12, 20, new Color(0.7f, 0.85f, 1.0f), true);

        scene.AddTriangle(new TriangleEntity(
            new Vector3(-5, 0, -5),
            new Vector3(-5, 0, 5),
            new Vector3(5, 0, 5),
            new Color(0.8f, 0.8f, 0.8f)
        ));

        scene.AddTriangle(new TriangleEntity(
            new Vector3(-5, 0, -5),
            new Vector3(5, 0, 5),
            new Vector3(5, 0, -5),
            new Color(0.8f, 0.8f, 0.8f)
        ));
    }

    public static void AddModelToScene(SceneEntity scene, ModelLoader modelLoader, string modelPath)
    {
        try
        {
            ModelData modelData = modelLoader.LoadModel(modelPath);
            scene.AddModel(modelData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load model {modelPath}: {ex.Message}");
        }
    }

    public static void CreateSceneWithModel(SceneEntity scene, ModelLoader modelLoader, string modelPath)
    {
        scene.AddLight(LightEntity.CreateDirectional(new Vector3(0.5f, -1.0f, 0.5f), Color.White, 0.5f, 0.1f));
        scene.AddLight(LightEntity.CreatePoint(new Vector3(-3, 4, 2), new Color(1.0f, 0.9f, 0.8f), 1.0f, 0.005f, 0.25f));

        AddModelToScene(scene, modelLoader, modelPath);

        scene.AddTriangle(new TriangleEntity(
            new Vector3(-10, 0, -10),
            new Vector3(-10, 0, 10),
            new Vector3(10, 0, 10),
            new Color(0.8f, 0.8f, 0.8f)
        ));

        scene.AddTriangle(new TriangleEntity(
            new Vector3(-10, 0, -10),
            new Vector3(10, 0, 10),
            new Vector3(10, 0, -10),
            new Color(0.8f, 0.8f, 0.8f)
        ));
    }
}