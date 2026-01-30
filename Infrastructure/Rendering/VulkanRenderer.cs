using Core.Rendering;
using Core.Scene;
using Infrastructure.Vulkan;

namespace Infrastructure.Rendering;

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