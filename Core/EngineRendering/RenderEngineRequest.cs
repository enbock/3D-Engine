using Core.Scene;

namespace Core.EngineRendering;

public class RenderEngineRequest(float deltaTime, SceneEntity scene)
{
    public float DeltaTime { get; set; } = deltaTime;
    public SceneEntity Scene => scene;
}