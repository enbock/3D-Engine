using VulkanEngine.Core.Math;

namespace VulkanEngine.Core.Entities;

public enum LightType
{
    Directional = 0,
    Point = 1,
    Ambient = 2
}

public class Light
{
    public LightType Type { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Direction { get; set; }
    public Color Color { get; set; }
    public float Intensity { get; set; }

    public Light(LightType type, Color? color = null, float intensity = 1.0f)
    {
        Type = type;
        Color = color ?? Math.Color.White;
        Intensity = intensity;
        Position = Vector3.Zero;
        Direction = new Vector3(0, -1, 0);
    }

    public static Light CreateDirectional(Vector3 direction, Color? color = null, float intensity = 1.0f)
    {
        return new Light(LightType.Directional, color, intensity)
        {
            Direction = direction.Normalized
        };
    }

    public static Light CreatePoint(Vector3 position, Color? color = null, float intensity = 1.0f)
    {
        return new Light(LightType.Point, color, intensity)
        {
            Position = position
        };
    }

    public static Light CreateAmbient(Color? color = null, float intensity = 0.3f)
    {
        return new Light(LightType.Ambient, color, intensity);
    }
}
