using Core.Scene;

namespace Core.EngineRendering;

public interface Renderer : IDisposable
{
    void Initialize();
    void Render(SceneEntity scene, float deltaTime);
    void Resize(int width, int height);
}