using Core.Math;
using Core.Scene;
using Core.Scene.Camera;

namespace Core.World;

public class WorldUseCase(SceneBuilderService sceneBuilderService)
{
    private readonly SceneEntity scene = new();

    public void Initialize()
    {
        scene.Camera = new CameraEntity(
            new Vector3(0, 0, 10),
            new Vector3()
        );

        sceneBuilderService.CreateSimpleScene(scene);
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