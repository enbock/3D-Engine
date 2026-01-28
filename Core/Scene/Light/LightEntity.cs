using Core.Math;

namespace Core.Scene.Light;

public enum LightType
{
    Directional = 0,
    Point = 1,
    Ambient = 2
}

public class LightEntity
{
    public LightType Type { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Direction { get; set; }
    public Color Color { get; set; }
    public float Intensity { get; set; }

    public LightEntity(LightType type, Color? color = null, float intensity = 1.0f)
    {
        Type = type;
        Color = color ?? Color.White;
        Intensity = intensity;
        Position = Vector3.Zero;
        Direction = new Vector3(0, -1, 0);
    }

    public static LightEntity CreateDirectional(Vector3 direction, Color? color = null, float intensity = 1.0f)
    {
        return new LightEntity(LightType.Directional, color, intensity)
        {
            Direction = direction.Normalized
        };
    }

    public static LightEntity CreatePoint(Vector3 position, Color? color = null, float intensity = 1.0f)
    {
        return new LightEntity(LightType.Point, color, intensity)
        {
            Position = position
        };
    }

    public static LightEntity CreateAmbient(Color? color = null, float intensity = 0.3f)
    {
        return new LightEntity(LightType.Ambient, color, intensity);
    }
}
