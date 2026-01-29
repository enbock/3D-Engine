namespace Core.EngineUpdate;

public class UpdateEngineRequest(float deltaTime)
{
    public float DeltaTime { get; set; } = deltaTime;
}