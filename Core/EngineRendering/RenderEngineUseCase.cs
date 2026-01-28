using Application.Container;
using Core.Rendering;
using Core.Scene;

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

        if (container.TryResolve<SceneEntity>(out var scene) && 
            container.TryResolve<Renderer>(out var renderer))
        {
            renderer?.Render(scene!, totalTime);
        }
    }
}
