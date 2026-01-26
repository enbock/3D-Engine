using VulkanEngine.Core.Math;

namespace VulkanEngine.Core.Entities;

public class Camera
{
    public Vector3 Position { get; set; }
    public Vector3 Target { get; set; }
    public Vector3 Up { get; set; }
    public float Fov { get; set; }
    public float AspectRatio { get; set; }
    public float Near { get; set; }
    public float Far { get; set; }

    public Camera(
        Vector3? position = null,
        Vector3? target = null,
        float fov = 45f,
        float aspectRatio = 16f / 9f,
        float near = 0.1f,
        float far = 100f)
    {
        Position = position ?? new Vector3(0, 2, 5);
        Target = target ?? Vector3.Zero;
        Up = Vector3.Up;
        Fov = fov * MathF.PI / 180f;
        AspectRatio = aspectRatio;
        Near = near;
        Far = far;
    }

    public Vector3 Forward => (Target - Position).Normalized;
    public Vector3 Right => Vector3.Cross(Forward, Up).Normalized;

    public void LookAt(Vector3 target)
    {
        Target = target;
    }

    public void SetPosition(Vector3 position)
    {
        Position = position;
    }

    public void SetAspectRatio(float aspectRatio)
    {
        AspectRatio = aspectRatio;
    }

    public void Orbit(float deltaX, float deltaY, float distance)
    {
        var theta = deltaX * 0.01f;
        var phi = deltaY * 0.01f;

        var x = distance * MathF.Sin(phi) * MathF.Cos(theta);
        var y = distance * MathF.Cos(phi);
        var z = distance * MathF.Sin(phi) * MathF.Sin(theta);

        Position = new Vector3(x, y, z);
    }

    public void Move(Vector3 direction, float speed)
    {
        Position += direction * speed;
    }

    public void MoveForward(float speed) => Move(Forward, speed);
    public void MoveBackward(float speed) => Move(-Forward, speed);
    public void MoveLeft(float speed) => Move(-Right, speed);
    public void MoveRight(float speed) => Move(Right, speed);
    public void MoveUp(float speed) => Move(Up, speed);
    public void MoveDown(float speed) => Move(-Up, speed);
}
