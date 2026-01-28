using Core.Math;

namespace Core.Scene.Geometry;

public class TriangleEntity
{
    public Vector3 V0 { get; set; }
    public Vector3 V1 { get; set; }
    public Vector3 V2 { get; set; }
    public Color Color { get; set; }

    public TriangleEntity(Vector3 v0, Vector3 v1, Vector3 v2, Color color)
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Color = color;
    }

    public Vector3 Normal
    {
        get
        {
            var edge1 = V1 - V0;
            var edge2 = V2 - V0;
            return Vector3.Cross(edge1, edge2).Normalized;
        }
    }

    public Vector3 Center => (V0 + V1 + V2) / 3.0f;
}
