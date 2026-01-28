using Application.Container;
using Core.Rendering;
using CoreScene = VulkanEngine.Core.Scene;

namespace Core.EngineRendering;

public class RenderEngineUseCase
{
    private readonly ServiceContainer container;
    private float totalTime;

    public RenderEngineUseCase(ServiceContainer container)
    {
        this.container = container;
    }

    public void Run(RenderEngineRequest request)
    {
        totalTime += request.DeltaTime;

        if (container.TryResolve<CoreScene.SceneEntity>(out var scene) && 
            container.TryResolve<Renderer>(out var renderer))
        {
            renderer?.Render(scene!, totalTime);
        }
    }
}
