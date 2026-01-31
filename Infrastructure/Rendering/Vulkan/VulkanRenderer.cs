using Core.EngineRendering;
using Core.Scene;

namespace Infrastructure.Rendering.Vulkan;

public class VulkanRenderer(InternalVulkanRenderer internalRenderer) : Renderer
{
    public void Initialize()
    {
        internalRenderer.Initialize();
    }

    public void Render(SceneEntity scene, float deltaTime)
    {
        internalRenderer.Render(scene, deltaTime);
    }

    public void Resize(int width, int height)
    {
        internalRenderer.Resize(width, height);
    }

    public void Dispose()
    {
        internalRenderer.Dispose();
    }
}