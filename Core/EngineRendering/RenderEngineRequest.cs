namespace Core.EngineRendering;

public class RenderEngineRequest(float deltaTime)
{
    public float DeltaTime { get; set; } = deltaTime;
}