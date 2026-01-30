using Core.Rendering;

namespace Core.EngineRendering;

public class RenderEngineUseCase(Renderer renderer)
{
    private float totalTime;

    public void Run(RenderEngineRequest request)
    {
        totalTime += request.DeltaTime;
        renderer.Render(request.Scene, totalTime);
    }
}