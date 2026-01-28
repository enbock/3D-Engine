namespace Core.EngineRendering;

public class RenderEngineRequest
{
    public float DeltaTime { get; set; }

    public RenderEngineRequest(float deltaTime)
    {
        DeltaTime = deltaTime;
    }
}
