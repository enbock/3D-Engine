using Application.Scene;
using Core.Assets;
using Core.Math;
using Core.Scene.Geometry;
using Core.Scene.Light;
using ModelLoader = Infrastructure.Assets.ModelLoader;

namespace Core.Scene;

public class SceneBuilderService
{
    public void CreateSimpleScene(SceneEntity scene)
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

    public void AddModelToScene(SceneEntity scene, ModelLoader modelLoader, string modelPath)
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

    public void CreateSceneWithModel(SceneEntity scene, ModelLoader modelLoader, string modelPath)
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

    public void CreateTeapotScene(SceneEntity scene, ModelLoader modelLoader)
    {
        scene.AddLight(LightEntity.CreateDirectional(new Vector3(0.5f, -1.0f, 0.5f), Color.White, 0.8f, 0.1f));
        scene.AddLight(LightEntity.CreatePoint(new Vector3(-3, 5, 2), new Color(1.0f, 0.9f, 0.8f), 1.5f, 0.005f, 0.25f));
        scene.AddLight(LightEntity.CreatePoint(new Vector3(3, 4, -2), new Color(0.8f, 0.9f, 1.0f), 1.0f, 0.005f, 0.25f));

        try
        {
            ModelData teapotModel = modelLoader.LoadModel("teapot.glb");

            foreach (MeshData mesh in teapotModel.Meshes)
            {
                MaterialEntity metalMaterial = new(
                    new Color(0.65f, 0.65f, 0.7f),
                    0.0f,
                    1.5f,
                    0.6f,
                    false
                )
                {
                    Metallic = 0.9f,
                    Roughness = 0.3f
                };

                float angleRadians = 90.0f * MathF.PI / 180.0f;
                float cosAngle = MathF.Cos(angleRadians);
                float sinAngle = MathF.Sin(angleRadians);

                for (int i = 0; i < mesh.Indices.Count; i += 3)
                {
                    VertexData v0 = mesh.Vertices[mesh.Indices[i]];
                    VertexData v1 = mesh.Vertices[mesh.Indices[i + 1]];
                    VertexData v2 = mesh.Vertices[mesh.Indices[i + 2]];

                    Vector3 rotatedPos0 = new(v0.Position.X, v0.Position.Y * cosAngle - v0.Position.Z * sinAngle, v0.Position.Y * sinAngle + v0.Position.Z * cosAngle);
                    Vector3 rotatedPos1 = new(v1.Position.X, v1.Position.Y * cosAngle - v1.Position.Z * sinAngle, v1.Position.Y * sinAngle + v1.Position.Z * cosAngle);
                    Vector3 rotatedPos2 = new(v2.Position.X, v2.Position.Y * cosAngle - v2.Position.Z * sinAngle, v2.Position.Y * sinAngle + v2.Position.Z * cosAngle);

                    Vector3 rotatedNorm0 = new(v0.Normal.X, v0.Normal.Y * cosAngle - v0.Normal.Z * sinAngle, v0.Normal.Y * sinAngle + v0.Normal.Z * cosAngle);
                    Vector3 rotatedNorm1 = new(v1.Normal.X, v1.Normal.Y * cosAngle - v1.Normal.Z * sinAngle, v1.Normal.Y * sinAngle + v1.Normal.Z * cosAngle);
                    Vector3 rotatedNorm2 = new(v2.Normal.X, v2.Normal.Y * cosAngle - v2.Normal.Z * sinAngle, v2.Normal.Y * sinAngle + v2.Normal.Z * cosAngle);

                    TriangleEntity triangle = new(
                        rotatedPos0, rotatedPos1, rotatedPos2,
                        metalMaterial,
                        rotatedNorm0.Normalized, rotatedNorm1.Normalized, rotatedNorm2.Normalized,
                        v0.UV, v1.UV, v2.UV
                    );

                    scene.Triangles.Add(triangle);
                }
            }

            Console.WriteLine($"Metallischer Teapot geladen: {teapotModel.Meshes.Count} Meshes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden des Teapots: {ex.Message}");
        }

        float ringRadius = 4.5f;

        GeometryGenerator.AddCylinder(scene, new Vector3(-ringRadius, 1, 0), 0.4f, 2.0f, 16, new Color(1.0f, 0.0f, 0.0f), ShadingMode.HalfSmooth);
        GeometryGenerator.AddSphere(scene, new Vector3(0, 1, -ringRadius), 0.7f, 12, 16, new Color(0.0f, 1.0f, 0.0f), ShadingMode.HalfSmooth);
        GeometryGenerator.AddCube(scene, new Vector3(ringRadius, 1, 0), 1.2f, new Color(0.0f, 0.0f, 1.0f));

        GeometryGenerator.AddGlassSphere(scene, new Vector3(-ringRadius * 0.7f, 0.6f, ringRadius * 0.7f), 0.6f, 16, 24, new Color(0.95f, 0.98f, 1.0f));
        GeometryGenerator.AddDiamondSphere(scene, new Vector3(ringRadius * 0.7f, 0.5f, ringRadius * 0.7f), 0.5f, 20, 32, new Color(1.0f, 1.0f, 1.0f));
        GeometryGenerator.AddWaterSphere(scene, new Vector3(0, 0.4f, ringRadius), 0.4f, 12, 20, new Color(0.7f, 0.85f, 1.0f), true);

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