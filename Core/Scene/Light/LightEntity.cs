using Core.Math;

namespace Core.Scene.Light;

public enum LightType
{
    Ambient = 0,
    Directional = 1,
    Point = 2
}

public class LightEntity(LightType type, Color? color = null, float intensity = 1.0f, float shadowSoftness = 0.03f)
{
    public LightType Type { get; set; } = type;
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Direction { get; set; } = new(0, -1, 0);
    public Color Color { get; set; } = color ?? Color.White;
    public float Intensity { get; set; } = intensity;
    public float ShadowSoftness { get; set; } = shadowSoftness;

    public static LightEntity CreateDirectional(Vector3 direction, Color? color = null, float intensity = 1.0f, float shadowSoftness = 0.03f)
    {
        return new LightEntity(LightType.Directional, color, intensity, shadowSoftness)
        {
            Direction = direction.Normalized
        };
    }

    public static LightEntity CreatePoint(Vector3 position, Color? color = null, float intensity = 1.0f, float shadowSoftness = 0.03f)
    {
        return new LightEntity(LightType.Point, color, intensity, shadowSoftness)
        {
            Position = position
        };
    }

    public static LightEntity CreateAmbient(Color? color = null, float intensity = 0.3f)
    {
        return new LightEntity(LightType.Ambient, color, intensity, 0.0f);
    }
}