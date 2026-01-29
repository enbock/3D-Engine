using Core.Math;

namespace Core.Scene.Camera;

public class CameraEntity
{
    public CameraEntity()
    {
        Position = new Vector3(0, 0, 10);
        Target = new Vector3(0, 0, 0);
        Up = new Vector3(0, 1, 0);
        Fov = 45.0f * MathF.PI / 180.0f;
        AspectRatio = 16.0f / 9.0f;
        Near = 0.1f;
        Far = 1000.0f;
    }

    public CameraEntity(Vector3 position, Vector3 target, float fovDegrees = 45.0f, float aspectRatio = 16.0f / 9.0f,
        float near = 0.1f, float far = 1000.0f)
    {
        Position = position;
        Target = target;
        Up = new Vector3(0, 1, 0);
        Fov = fovDegrees * MathF.PI / 180.0f;
        AspectRatio = aspectRatio;
        Near = near;
        Far = far;
    }

    public Vector3 Position { get; set; }
    public Vector3 Target { get; set; }
    public Vector3 Up { get; set; }
    public float Fov { get; set; }
    public float AspectRatio { get; set; }
    public float Near { get; set; }
    public float Far { get; set; }

    public Vector3 Forward
    {
        get
        {
            Vector3 forward = (Target - Position).Normalized;
            return forward;
        }
    }

    public Vector3 Right
    {
        get
        {
            Vector3 forward = Forward;
            Vector3 right = Vector3.Cross(forward, Up).Normalized;
            return right;
        }
    }

    public void SetAspectRatio(float aspectRatio)
    {
        AspectRatio = aspectRatio;
    }
}