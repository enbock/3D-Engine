using Application;
using Core.Rendering;
using Core.Scene;
using Application.Window;
using Infrastructure.Vulkan;

namespace Infrastructure.Rendering;

public unsafe class VulkanRenderer : Renderer
{
    private readonly WindowManagerService windowManager;
    private readonly EngineConfig config;
    private readonly InternalVulkanRenderer internalRenderer;

    public VulkanRenderer(WindowManagerService windowManager, EngineConfig config)
    {
        this.windowManager = windowManager;
        this.config = config;
        this.internalRenderer = new InternalVulkanRenderer(windowManager, config);
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
