using Core.Assets;
using Core.Math;
using Core.Scene.Geometry;
using Core.Scene.Light;
using Core.Scene.Transform;
using ModelLoader = Infrastructure.Assets.ModelLoader;

namespace Core.Scene;

public class SceneBuilderService
{
    public void CreateTeapotScene(SceneEntity scene, ModelLoader modelLoader)
    {
        scene.AddLight(LightEntity.CreateDirectional(new Vector3(0.5f, -1.0f, 0.5f), Color.White, 0.8f, 0.1f));
        scene.AddLight(LightEntity.CreatePoint(new Vector3(-3, 5, 2), new Color(1.0f, 0.9f, 0.8f), 1.5f, 0.005f, 0.25f));
        scene.AddLight(LightEntity.CreatePoint(new Vector3(3, 4, -2), new Color(0.8f, 0.9f, 1.0f), 1.0f, 0.005f, 0.25f));

        LoadTeapot(scene, modelLoader);
        LoadDragon(scene, modelLoader);
        LoadEnterprise(scene, modelLoader);

        float ringRadius = 4f;

        GeometryGenerator.AddCylinder(scene, new Vector3(-ringRadius, 1, 0), 0.4f, 2.0f, 16, new Color(1.0f, 0.0f, 0.0f), ShadingMode.HalfSmooth);
        GeometryGenerator.AddSphere(scene, new Vector3(0, 1, -ringRadius), 0.7f, 12, 16, new Color(0.0f, 1.0f, 0.0f), ShadingMode.HalfSmooth);
        GeometryGenerator.AddCube(scene, new Vector3(ringRadius, 1, 0), 1.2f, new Color(0.0f, 0.0f, 1.0f));

        GeometryGenerator.AddGlassSphere(scene, new Vector3(-ringRadius * 0.7f, 0.6f, ringRadius * 0.7f), 0.6f, 16, 24, new Color(0.95f, 0.98f, 1.0f));
        GeometryGenerator.AddDiamondSphere(scene, new Vector3(ringRadius * 0.7f, 0.5f, ringRadius * 0.7f), 0.5f, 20, 32, new Color(1.0f, 1.0f, 1.0f));
        GeometryGenerator.AddWaterSphere(scene, new Vector3(0, 0.4f, ringRadius), 0.4f, 12, 20, new Color(1f, 0.35f, 0.3f), true);

        scene.AddTriangle(new TriangleEntity(
            new Vector3(-100, 0, -100),
            new Vector3(-100, 0, 100),
            new Vector3(100, 0, 100),
            new Color(0.8f, 0.8f, 0.8f)
        ));

        scene.AddTriangle(new TriangleEntity(
            new Vector3(-100, 0, -100),
            new Vector3(100, 0, 100),
            new Vector3(100, 0, -100),
            new Color(0.8f, 0.8f, 0.8f)
        ));
    }

    private static void LoadDragon(SceneEntity scene, ModelLoader modelLoader)
    {
        try
        {
            ModelData dragonModel = modelLoader.LoadModel("stanford_dragon_pbr.glb");

            TransformService dragonTransformService = new();
            TransformData dragonTransform = new(
                new Vector3(80.0f, 0.0f, -120.0f),
                Vector3.Zero,
                new Vector3(0.06f, 0.06f, 0.06f)
            );

            foreach (MeshData mesh in dragonModel.Meshes)
            {
                MaterialEntity dragonMaterial = new(
                    new Color(0.6f, 1f, 0.5f),
                    0.25f,
                    1.50f,
                    1f
                )
                {
                    Metallic = 0.2f,
                    Roughness = 0.6f
                };

                for (int i = 0; i < mesh.Indices.Count; i += 3)
                {
                    VertexData v0 = mesh.Vertices[mesh.Indices[i]];
                    VertexData v1 = mesh.Vertices[mesh.Indices[i + 1]];
                    VertexData v2 = mesh.Vertices[mesh.Indices[i + 2]];

                    TriangleEntity triangle = new(
                        v0.Position, v1.Position, v2.Position,
                        dragonMaterial,
                        v0.Normal, v1.Normal, v2.Normal,
                        v0.UV, v1.UV, v2.UV
                    );

                    TriangleEntity transformed = dragonTransformService.ApplyTransform(triangle, dragonTransform);

                    scene.Triangles.Add(transformed);
                }
            }

            Console.WriteLine($"Stanford Dragon geladen: {dragonModel.Meshes.Count} Meshes (skaliert auf 0.1%)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden des Stanford Dragon: {ex.Message}");
        }
    }

    private static void LoadTeapot(SceneEntity scene, ModelLoader modelLoader)
    {
        try
        {
            ModelData teapotModel = modelLoader.LoadModel("teapot.glb");

            foreach (MeshData mesh in teapotModel.Meshes)
            {
                MaterialEntity metalMaterial = new(
                    new Color(0.65f, 0.65f, 0.7f),
                    0.0f,
                    1.0f,
                    0.3f
                )
                {
                    Metallic = 0.9f,
                    Roughness = 0.3f
                };


                TransformService transformService = new();
                TransformData teapotTransform = new(
                    new Vector3(0, 0, 0),
                    new Vector3(MathF.PI / 2.0f, 0, 0),
                    Vector3.One
                );

                for (int i = 0; i < mesh.Indices.Count; i += 3)
                {
                    VertexData v0 = mesh.Vertices[mesh.Indices[i]];
                    VertexData v1 = mesh.Vertices[mesh.Indices[i + 1]];
                    VertexData v2 = mesh.Vertices[mesh.Indices[i + 2]];

                    TriangleEntity triangle = new(
                        v0.Position, v1.Position, v2.Position,
                        metalMaterial,
                        v0.Normal, v1.Normal, v2.Normal,
                        v0.UV, v1.UV, v2.UV
                    );

                    TriangleEntity transformed = transformService.ApplyTransform(triangle, teapotTransform);

                    scene.Triangles.Add(transformed);
                }
            }

            Console.WriteLine($"Metallischer Teapot geladen: {teapotModel.Meshes.Count} Meshes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden des Teapots: {ex.Message}");
        }
    }

    private static void LoadEnterprise(SceneEntity scene, ModelLoader modelLoader)
    {
        try
        {
            ModelData teapotModel = modelLoader.LoadModel("star_trek_-_galaxy_class.glb");

            foreach (MeshData mesh in teapotModel.Meshes)
            {
                MaterialEntity metalMaterial = new(
                    new Color(0.65f, 0.65f, 0.7f)
                )
                {
                    Metallic = 0.9f,
                    Roughness = 0.8f
                };


                TransformService transformService = new();
                TransformData teapotTransform = new(
                    new Vector3(100f, 350.0f, -250f),
                    new Vector3(-0.2f, 1f, 0),
                    new Vector3(0.03f, 0.03f, 0.03f)
                );

                for (int i = 0; i < mesh.Indices.Count; i += 3)
                {
                    VertexData v0 = mesh.Vertices[mesh.Indices[i]];
                    VertexData v1 = mesh.Vertices[mesh.Indices[i + 1]];
                    VertexData v2 = mesh.Vertices[mesh.Indices[i + 2]];

                    TriangleEntity triangle = new(
                        v0.Position, v1.Position, v2.Position,
                        metalMaterial,
                        v0.Normal, v1.Normal, v2.Normal,
                        v0.UV, v1.UV, v2.UV
                    );

                    TriangleEntity transformed = transformService.ApplyTransform(triangle, teapotTransform);

                    scene.Triangles.Add(transformed);
                }
            }

            Console.WriteLine($"Metallischer Teapot geladen: {teapotModel.Meshes.Count} Meshes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden des Teapots: {ex.Message}");
        }
    }
}