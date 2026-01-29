namespace Core.EngineUpdate;

public class UpdateEngineRequest
{
    public UpdateEngineRequest(float deltaTime)
    {
        DeltaTime = deltaTime;
    }

    public float DeltaTime { get; set; }
}