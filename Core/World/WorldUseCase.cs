using Core.Math;
using Core.Scene;
using Core.Scene.Camera;

namespace Core.World;

public class WorldUseCase(SceneBuilderService sceneBuilderService)
{
    private readonly CameraEntity camera = new(
        new Vector3(0, 0, 10),
        new Vector3()
    );

    private readonly SceneEntity scene = new();

    public void Initialize()
    {
        sceneBuilderService.CreateSimpleScene(scene);
    }

    public void UpdateAspectRatio(float aspectRatio)
    {
        camera.SetAspectRatio(aspectRatio);
    }

    public CameraEntity GetCamera()
    {
        return camera;
    }

    public SceneEntity GetScene()
    {
        return scene;
    }
}