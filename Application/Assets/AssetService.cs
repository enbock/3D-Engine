using Core.Assets;
using Core.Scene;
using Infrastructure.Assets;

namespace Application.Assets;

public class AssetService
{
    private readonly ModelLoader _modelLoader;

    public AssetService(ModelLoader modelLoader)
    {
        _modelLoader = modelLoader;
    }

    public void LoadModelIntoScene(string modelPath, SceneEntity scene)
    {
        ModelData modelData = _modelLoader.LoadModel(modelPath);
        scene.AddModel(modelData);
        Console.WriteLine($"Added model '{modelData.Name}' to scene with {modelData.Meshes.Count} meshes");
    }

    public ModelData LoadModel(string modelPath)
    {
        return _modelLoader.LoadModel(modelPath);
    }
}