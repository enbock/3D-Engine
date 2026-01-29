namespace Core.EngineRendering;

public class RenderEngineRequest
{
    public RenderEngineRequest(float deltaTime)
    {
        DeltaTime = deltaTime;
    }

    public float DeltaTime { get; set; }
}