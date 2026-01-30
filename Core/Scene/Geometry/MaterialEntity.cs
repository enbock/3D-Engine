using Core.Math;

namespace Core.Scene.Geometry;

public class MaterialEntity(Color color, float transparency = 0.0f, float ior = 1.0f, float reflectivity = 0.0f)
{
    public Color Color { get; set; } = color;
    public float Transparency { get; set; } = System.Math.Clamp(transparency, 0.0f, 1.0f);
    public float IndexOfRefraction { get; set; } = System.Math.Max(1.0f, ior);
    public float Reflectivity { get; set; } = System.Math.Clamp(reflectivity, 0.0f, 1.0f);

    public bool IsTransparent => Transparency > 0.01f;

    public static MaterialEntity Opaque(Color color)
    {
        return new MaterialEntity(color);
    }

    public static MaterialEntity Glass(Color color, float ior = 1.52f)
    {
        return new MaterialEntity(color, 0.95f, ior, 0.1f);
    }

    public static MaterialEntity Water(Color color)
    {
        return new MaterialEntity(color, 0.9f, 1.33f, 0.05f);
    }

    public static MaterialEntity Diamond(Color color)
    {
        return new MaterialEntity(color, 0.98f, 2.42f, 0.15f);
    }
}