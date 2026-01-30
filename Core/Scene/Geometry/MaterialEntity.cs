using Core.Math;

namespace Core.Scene.Geometry;

public class MaterialEntity(Color color, float transparency = 0.0f, float ior = 1.0f, float reflectivity = 0.0f, bool enableSchlieren = false)
{
    public Color Color { get; set; } = color;
    public float Transparency { get; set; } = System.Math.Clamp(transparency, 0.0f, 1.0f);
    public float IndexOfRefraction { get; set; } = System.Math.Max(1.0f, ior);
    public float Reflectivity { get; set; } = System.Math.Clamp(reflectivity, 0.0f, 1.0f);
    public bool EnableSchlieren { get; set; } = enableSchlieren;

    public bool IsTransparent => Transparency > 0.01f;

    public static MaterialEntity Opaque(Color color)
    {
        return new MaterialEntity(color);
    }

    public static MaterialEntity Glass(Color color, float ior = 1.52f, bool schlieren = false)
    {
        return new MaterialEntity(color, 0.95f, ior, 0.1f, schlieren);
    }

    public static MaterialEntity Water(Color color, bool schlieren = false)
    {
        return new MaterialEntity(color, 0.9f, 1.33f, 0.05f, schlieren);
    }

    public static MaterialEntity Diamond(Color color, bool schlieren = false)
    {
        return new MaterialEntity(color, 0.98f, 2.42f, 0.15f, schlieren);
    }
}