using Application;
using Application.Window;
using Core.Rendering;
using Core.Scene;
using Infrastructure.Vulkan;

namespace Infrastructure.Rendering;

public class VulkanRenderer(WindowManagerService windowManager, EngineConfig config) : Renderer
{
    private readonly InternalVulkanRenderer internalRenderer = new(windowManager, config);

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