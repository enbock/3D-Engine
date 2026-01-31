using Core.Math;
using Core.Scene;
using Core.Scene.Camera;
using Infrastructure.Assets;

namespace Core.World;

public class WorldUseCase(SceneBuilderService? sceneBuilder = null, ModelLoader? modelLoader = null)
{
    private readonly SceneEntity scene = new();
    private ModelLoader _modelLoader = modelLoader!;
    private SceneBuilderService _sceneBuilder = sceneBuilder!;

    public void SetSceneBuilder(SceneBuilderService sceneBuilder)
    {
        _sceneBuilder = sceneBuilder;
    }

    public void SetModelLoader(ModelLoader modelLoader)
    {
        _modelLoader = modelLoader;
    }

    public void InitializeWithTeapot()
    {
        scene.Camera = new CameraEntity(
            new Vector3(0, 3, 10),
            new Vector3(0, 1.5f, 0)
        );

        _sceneBuilder.CreateTeapotScene(scene, _modelLoader);
    }

    public void UpdateAspectRatio(float aspectRatio)
    {
        scene.Camera.SetAspectRatio(aspectRatio);
    }

    public CameraEntity GetCamera()
    {
        return scene.Camera;
    }

    public SceneEntity GetScene()
    {
        return scene;
    }
}