using Core.Input;
using Core.Math;
using Core.Scene.Camera;

namespace Application.CameraControl;

public class CameraControlUseCase
{
    private const float moveSpeed = 5.0f;
    private const float lookSpeed = 0.003f;
    private readonly CameraEntity camera;
    private readonly InputHandler input;
    private float pitch;

    private float yaw;

    public CameraControlUseCase(CameraEntity camera, InputHandler input)
    {
        this.camera = camera;
        this.input = input;

        Vector3 direction = camera.Forward;
        yaw = MathF.Atan2(direction.Z, direction.X);
        pitch = MathF.Asin(direction.Y);
    }

    public void Run(float deltaTime)
    {
        HandleMovement(deltaTime);
        HandleLook();
        input.ResetMouseDelta();
    }

    private void HandleMovement(float deltaTime)
    {
        Vector3 movement = input.GetMovementInput();
        if (movement == Vector3.Zero) return;

        Vector3 forward = camera.Forward;
        Vector3 right = camera.Right;
        Vector3 up = camera.Up;

        Vector3 velocity = (forward * -movement.Z + right * movement.X + up * movement.Y) * moveSpeed * deltaTime;

        camera.Position += velocity;
        camera.Target += velocity;
    }

    private void HandleLook()
    {
        Vector3 delta = input.GetMouseDelta();
        if (delta == Vector3.Zero) return;

        yaw += delta.X * lookSpeed;
        pitch -= delta.Y * lookSpeed;

        pitch = Math.Clamp(pitch, -MathF.PI / 2 + 0.01f, MathF.PI / 2 - 0.01f);

        Vector3 direction = new(
            MathF.Cos(pitch) * MathF.Cos(yaw),
            MathF.Sin(pitch),
            MathF.Cos(pitch) * MathF.Sin(yaw)
        );

        camera.Target = camera.Position + direction.Normalized;
    }
}