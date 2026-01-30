using Core.Math;
using Core.Scene.Camera;

namespace Core.CameraControl;

public class UpdateCameraMovementRequest(Vector3 movement, float deltaTime, CameraEntity camera)
{
    public Vector3 Movement { get; } = movement;
    public float DeltaTime { get; } = deltaTime;
    public CameraEntity Camera { get; } = camera;
}