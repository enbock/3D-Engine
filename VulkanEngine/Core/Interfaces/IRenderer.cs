namespace VulkanEngine.Core.Interfaces;

public interface IRenderer : IDisposable
{
    void Initialize();
    void Render(Scene scene, float deltaTime);
    void Resize(int width, int height);
}
