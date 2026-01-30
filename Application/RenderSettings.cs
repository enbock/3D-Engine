namespace Application;

public class RenderSettings
{
    public int MaxBounces { get; private init; } = 3;
    public bool EnableShadows { get; private init; } = true;
    public bool EnableReflections { get; private init; } = true;
    public float ReflectionStrength { get; private init; } = 0.5f;
    public int ShadowSamples { get; private init; } = 4;
    public float ShadowSoftness { get; private init; } = 0.05f;

    public static RenderSettings Default => new()
    {
        MaxBounces = 3,
        EnableShadows = true,
        EnableReflections = true,
        ReflectionStrength = 0.5f,
        ShadowSamples = 12,
        ShadowSoftness = 0.04f
    };

    public static RenderSettings Performance => new()
    {
        MaxBounces = 2,
        EnableShadows = true,
        EnableReflections = true,
        ReflectionStrength = 0.4f,
        ShadowSamples = 8,
        ShadowSoftness = 0.03f
    };

    public static RenderSettings UltraPerformance => new()
    {
        MaxBounces = 0,
        EnableShadows = false,
        EnableReflections = false,
        ReflectionStrength = 0.0f,
        ShadowSamples = 0,
        ShadowSoftness = 0.0f
    };

    public static RenderSettings Quality => new()
    {
        MaxBounces = 5,
        EnableShadows = true,
        EnableReflections = true,
        ReflectionStrength = 0.7f,
        ShadowSamples = 8,
        ShadowSoftness = 0.1f
    };
}