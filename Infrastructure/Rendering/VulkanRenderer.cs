using Application;
using Application.Window;
using Core.Rendering;
using Core.Scene;
using Infrastructure.Vulkan;

namespace Infrastructure.Rendering;

public class VulkanRenderer : Renderer
{
    private readonly EngineConfig config;
    private readonly InternalVulkanRenderer internalRenderer;
    private readonly WindowManagerService windowManager;

    public VulkanRenderer(WindowManagerService windowManager, EngineConfig config)
    {
        this.windowManager = windowManager;
        this.config = config;
        internalRenderer = new InternalVulkanRenderer(windowManager, config);
    }

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