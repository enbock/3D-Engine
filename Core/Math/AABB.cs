namespace Core.Math;

public struct Aabb(Vector3 min, Vector3 max)
{
    public Vector3 Min { get; set; } = min;
    public Vector3 Max { get; set; } = max;

    public Vector3 Center => (Min + Max) * 0.5f;
    public Vector3 Size => Max - Min;

    public float SurfaceArea
    {
        get
        {
            Vector3 size = Size;
            return 2.0f * (size.X * size.Y + size.Y * size.Z + size.Z * size.X);
        }
    }

    public bool Intersects(Aabb other)
    {
        return Min.X <= other.Max.X && Max.X >= other.Min.X &&
               Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
               Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
    }

    public bool Contains(Vector3 point)
    {
        return point.X >= Min.X && point.X <= Max.X &&
               point.Y >= Min.Y && point.Y <= Max.Y &&
               point.Z >= Min.Z && point.Z <= Max.Z;
    }

    public void Expand(Vector3 point)
    {
        Min = new Vector3(
            System.Math.Min(Min.X, point.X),
            System.Math.Min(Min.Y, point.Y),
            System.Math.Min(Min.Z, point.Z)
        );
        Max = new Vector3(
            System.Math.Max(Max.X, point.X),
            System.Math.Max(Max.Y, point.Y),
            System.Math.Max(Max.Z, point.Z)
        );
    }

    public void Expand(Aabb other)
    {
        Min = new Vector3(
            System.Math.Min(Min.X, other.Min.X),
            System.Math.Min(Min.Y, other.Min.Y),
            System.Math.Min(Min.Z, other.Min.Z)
        );
        Max = new Vector3(
            System.Math.Max(Max.X, other.Max.X),
            System.Math.Max(Max.Y, other.Max.Y),
            System.Math.Max(Max.Z, other.Max.Z)
        );
    }


    public static Aabb Empty => new(
        new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
        new Vector3(float.MinValue, float.MinValue, float.MinValue)
    );
}