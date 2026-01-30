using Core.Math;
using Core.Scene.Camera;

namespace Core.CameraControl;

public class UpdateCameraLookRequest(Vector3 mouseDelta, CameraEntity camera)
{
    public Vector3 MouseDelta { get; } = mouseDelta;
    public CameraEntity Camera { get; } = camera;
}