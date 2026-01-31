using Core.Math;
using Core.Scene;
using Core.Scene.Camera;
using Infrastructure.Assets;

namespace Core.World;

public class WorldUseCase
{
    private readonly SceneEntity scene = new();
    private ModelLoader? _modelLoader;

    public void Initialize()
    {
        scene.Camera = new CameraEntity(
            new Vector3(0, 0, 10),
            new Vector3()
        );

        SceneBuilderService.CreateSimpleScene(scene);
    }

    public void SetModelLoader(ModelLoader modelLoader)
    {
        _modelLoader = modelLoader;
    }

    public void LoadModel(string modelPath)
    {
        if (_modelLoader == null)
        {
            Console.WriteLine("ModelLoader not set. Call SetModelLoader first.");
            return;
        }

        SceneBuilderService.AddModelToScene(scene, _modelLoader, modelPath);
    }

    public void InitializeWithModel(string modelPath)
    {
        scene.Camera = new CameraEntity(
            new Vector3(0, 2, 8),
            new Vector3(0, 0, 0)
        );

        if (_modelLoader != null)
        {
            SceneBuilderService.CreateSceneWithModel(scene, _modelLoader, modelPath);
        }
        else
        {
            SceneBuilderService.CreateSimpleScene(scene);
        }
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