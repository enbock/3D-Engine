using Silk.NET.Input;
using VulkanEngine.Core;
using VulkanEngine.Core.Entities;

namespace VulkanEngine.Infrastructure.Input;

public class CameraController
{
    private readonly Camera _camera;
    private readonly InputHandler _input;
    private float _moveSpeed = 5.0f;
    private float _lookSpeed = 0.003f;
    private float _yaw;
    private float _pitch;

    public CameraController(Camera camera, InputHandler input)
    {
        _camera = camera;
        _input = input;

        var direction = _camera.Forward;
        _yaw = MathF.Atan2(direction.Z, direction.X);
        _pitch = MathF.Asin(direction.Y);
    }

    public void Update(float deltaTime)
    {
        var speed = _moveSpeed * deltaTime;

        if (_input.IsKeyDown(Key.W))
            _camera.MoveForward(speed);
        if (_input.IsKeyDown(Key.S))
            _camera.MoveBackward(speed);
        if (_input.IsKeyDown(Key.A))
            _camera.MoveLeft(speed);
        if (_input.IsKeyDown(Key.D))
            _camera.MoveRight(speed);
        if (_input.IsKeyDown(Key.Space))
            _camera.MoveUp(speed);
        if (_input.IsKeyDown(Key.ShiftLeft))
            _camera.MoveDown(speed);

        if (_input.IsMouseButtonPressed(MouseButton.Right))
        {
            var delta = _input.GetMouseDelta();

            _yaw += delta.X * _lookSpeed;
            _pitch -= delta.Y * _lookSpeed;

            _pitch = Math.Clamp(_pitch, -MathF.PI / 2 + 0.01f, MathF.PI / 2 - 0.01f);

            var direction = new Core.Math.Vector3(
                MathF.Cos(_pitch) * MathF.Cos(_yaw),
                MathF.Sin(_pitch),
                MathF.Cos(_pitch) * MathF.Sin(_yaw)
            );

            _camera.Target = _camera.Position + direction;
        }
    }
}
