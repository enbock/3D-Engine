using Core.Math;
using Core.Scene;
using Core.Scene.Geometry;
using Core.Scene.Light;

namespace Application.Scene;

public class SceneBuilderService
{
    public SceneEntity CreateDemoScene()
    {
        return CreateSimpleScene();
    }

    public SceneEntity CreateSimpleScene()
    {
        var scene = new SceneEntity();

        scene.Camera.Position = new Vector3(0, 3, 10);
        scene.Camera.Target = new Vector3(0, 1, 0);

        scene.AddLight(LightEntity.CreateAmbient(Color.White, 0f));
        scene.AddLight(LightEntity.CreateDirectional(new Vector3(0.5f, -1.0f, 0.3f), Color.White, 0.5f));
        scene.AddLight(LightEntity.CreatePoint(new Vector3(-3, 4, 2), new Color(1.0f, 0.9f, 0.8f)));

        scene.AddTriangle(new TriangleEntity(
            new Vector3(-2, 0, -1),
            new Vector3(-1, 2, -1),
            new Vector3(-1, 0, -1),
            new Color(1.0f, 0.0f, 0.0f)
        ));

        scene.AddTriangle(new TriangleEntity(
            new Vector3(0, 0, -0.5f),
            new Vector3(0, 2, 0),
            new Vector3(0, 0, 0.5f),
            new Color(0.0f, 1.0f, 0.0f)
        ));

        scene.AddTriangle(new TriangleEntity(
            new Vector3(2, 0, 0.5f),
            new Vector3(2, 2, 0),
            new Vector3(2, 0, -0.5f),
            new Color(0.0f, 0.0f, 1.0f)
        ));

        scene.AddTriangle(new TriangleEntity(
            new Vector3(-5, 0, -5),
            new Vector3(5, 0, -5),
            new Vector3(5, 0, 5),
            new Color(0.8f, 0.8f, 0.8f)
        ));

        scene.AddTriangle(new TriangleEntity(
            new Vector3(-5, 0, -5),
            new Vector3(5, 0, 5),
            new Vector3(-5, 0, 5),
            new Color(0.8f, 0.8f, 0.8f)
        ));

        return scene;
    }
}