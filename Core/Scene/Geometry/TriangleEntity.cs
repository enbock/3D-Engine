using Core.Math;

namespace Core.Scene.Geometry;

public class TriangleEntity(Vector3 v0, Vector3 v1, Vector3 v2, Color color)
{
    public Vector3 V0 { get; set; } = v0;
    public Vector3 V1 { get; set; } = v1;
    public Vector3 V2 { get; set; } = v2;
    public Color Color { get; set; } = color;

    public Vector3 Normal
    {
        get
        {
            Vector3 edge1 = V1 - V0;
            Vector3 edge2 = V2 - V0;
            return Vector3.Cross(edge1, edge2).Normalized;
        }
    }

    public Vector3 Center => (V0 + V1 + V2) / 3.0f;
}