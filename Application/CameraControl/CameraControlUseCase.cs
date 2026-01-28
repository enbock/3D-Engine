using Silk.NET.Input;
using Core.Scene.Camera;
using Core.Input;
using Core.Math;

namespace Application.CameraControl;

public class CameraControlUseCase
{
    private readonly CameraEntity camera;
    private readonly InputHandler input;
    
    private float yaw;
    private float pitch;
    private const float moveSpeed = 5.0f;
    private const float lookSpeed = 0.003f;

    public CameraControlUseCase(CameraEntity camera, InputHandler input)
    {
        this.camera = camera;
        this.input = input;

        var direction = camera.Forward;
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
        var movement = input.GetMovementInput();
        if (movement == Vector3.Zero) return;

        var forward = camera.Forward;
        var right = camera.Right;
        var up = camera.Up;

        var velocity = (forward * -movement.Z + right * movement.X + up * movement.Y) * moveSpeed * deltaTime;

        camera.Position += velocity;
        camera.Target += velocity;
    }

    private void HandleLook()
    {
        var delta = input.GetMouseDelta();
        if (delta == Vector3.Zero) return;

        yaw += delta.X * lookSpeed;
        pitch -= delta.Y * lookSpeed;

        pitch = Math.Clamp(pitch, -MathF.PI / 2 + 0.01f, MathF.PI / 2 - 0.01f);

        var direction = new Vector3(
            MathF.Cos(pitch) * MathF.Cos(yaw),
            MathF.Sin(pitch),
            MathF.Cos(pitch) * MathF.Sin(yaw)
        );

        camera.Target = camera.Position + direction.Normalized;
    }
}
