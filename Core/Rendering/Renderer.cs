using Core.Scene;

namespace Core.Rendering;

public interface Renderer : IDisposable
{
    void Initialize();
    void Render(SceneEntity scene, float deltaTime);
    void Resize(int width, int height);
}