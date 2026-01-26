using VulkanEngine.Core.Math;

namespace VulkanEngine.Core.Entities;

public class Triangle
{
    public Vector3 V0 { get; set; }
    public Vector3 V1 { get; set; }
    public Vector3 V2 { get; set; }
    public Color Color { get; set; }

    public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Color? color = null)
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Color = color ?? Math.Color.White;
    }

    public Vector3 Normal
    {
        get
        {
            var e1 = V1 - V0;
            var e2 = V2 - V0;
            return Vector3.Cross(e1, e2).Normalized;
        }
    }

    public Vector3 Center => (V0 + V1 + V2) / 3f;

    public static Triangle CreateQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Color? color = null)
    {
        return new Triangle(p0, p1, p2, color);
    }
}
