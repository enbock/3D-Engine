namespace Core.Math;

public class TransformData
{
    public TransformData()
    {
        Position = Vector3.Zero;
        Rotation = Vector3.Zero;
        Scale = Vector3.One;
    }

    public TransformData(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public static TransformData Default => new();

    public Matrix4X4 GetMatrix()
    {
        return Matrix4X4.Translation(Position) * Matrix4X4.Rotation(Rotation) * Matrix4X4.Scale(Scale);
    }

    public void Translate(Vector3 offset)
    {
        Position += offset;
    }

    public void Rotate(Vector3 angles)
    {
        Rotation += angles;
    }

    public void ScaleBy(Vector3 factor)
    {
        Scale = new Vector3(Scale.X * factor.X, Scale.Y * factor.Y, Scale.Z * factor.Z);
    }

    public void SetPosition(Vector3 position)
    {
        Position = position;
    }

    public void SetRotation(Vector3 rotation)
    {
        Rotation = rotation;
    }

    public void SetScale(Vector3 scale)
    {
        Scale = scale;
    }

    public TransformData Clone()
    {
        return new TransformData(Position, Rotation, Scale);
    }
}