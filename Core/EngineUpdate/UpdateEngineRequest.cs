namespace Core.EngineUpdate;

public class UpdateEngineRequest
{
    public float DeltaTime { get; set; }

    public UpdateEngineRequest(float deltaTime)
    {
        DeltaTime = deltaTime;
    }
}
