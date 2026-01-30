using Core.Math;
using Core.Scene.Camera;

namespace Core.CameraControl;

public class CameraControlUseCase
{
    private const float MoveSpeed = 5.0f;
    private const float LookSpeed = 0.003f;
    private float pitch;
    private float yaw;

    public void Initialize(CameraEntity camera)
    {
        Vector3 direction = camera.Forward;
        yaw = MathF.Atan2(direction.Z, direction.X);
        pitch = MathF.Asin(direction.Y);
    }

    public void UpdateMovement(UpdateCameraMovementRequest request)
    {
        CameraEntity camera = request.Camera;
        Vector3 movement = request.Movement;
        if (movement == Vector3.Zero) return;

        Vector3 forward = camera.Forward;
        Vector3 right = camera.Right;
        Vector3 up = camera.Up;

        Vector3 velocity = (forward * -movement.Z + right * movement.X + up * movement.Y) * MoveSpeed * request.DeltaTime;

        camera.Position += velocity;

        Vector3 direction = new(
            MathF.Cos(pitch) * MathF.Cos(yaw),
            MathF.Sin(pitch),
            MathF.Cos(pitch) * MathF.Sin(yaw)
        );
        camera.Target = camera.Position + direction.Normalized;
    }

    public void UpdateLook(UpdateCameraLookRequest request)
    {
        Vector3 delta = request.MouseDelta;
        if (delta == Vector3.Zero) return;

        yaw += delta.X * LookSpeed;
        pitch -= delta.Y * LookSpeed;

        pitch = System.Math.Max(-MathF.PI / 2 + 0.01f, System.Math.Min(MathF.PI / 2 - 0.01f, pitch));

        Vector3 direction = new(
            MathF.Cos(pitch) * MathF.Cos(yaw),
            MathF.Sin(pitch),
            MathF.Cos(pitch) * MathF.Sin(yaw)
        );

        CameraEntity camera = request.Camera;
        camera.Target = camera.Position + direction.Normalized;
    }
}