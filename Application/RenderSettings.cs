namespace Application;

public enum ToneMappingOperator
{
    None = 0,
    Reinhard = 1,
    AcesFilmic = 2,
    Uncharted2 = 3
}

public class RenderSettings
{
    public int MaxBounces { get; private init; } = 3;
    public bool EnableShadows { get; private init; } = true;
    public bool EnableReflections { get; private init; } = true;
    public float ReflectionStrength { get; private init; } = 0.5f;
    public int ShadowSamples { get; private init; } = 4;
    public bool EnableGi { get; private init; } = true;
    public int GiSamples { get; private init; } = 4;
    public float GiStrength { get; private init; } = 0.5f;
    public bool EnableCaustics { get; private init; } = true;

    public static RenderSettings Default => new()
    {
        MaxBounces = 3,
        EnableShadows = true,
        EnableReflections = true,
        ReflectionStrength = 0.5f,
        ShadowSamples = 8,
        EnableGi = true,
        GiSamples = 2,
        GiStrength = 0.5f,
        EnableCaustics = true
    };

    public static RenderSettings Performance => new()
    {
        MaxBounces = 2,
        EnableShadows = true,
        EnableReflections = true,
        ReflectionStrength = 0.5f,
        ShadowSamples = 4,
        EnableGi = true,
        GiSamples = 1,
        GiStrength = 0.5f,
        EnableCaustics = true
    };

    public static RenderSettings UltraPerformance => new()
    {
        MaxBounces = 0,
        EnableShadows = false,
        EnableReflections = false,
        ReflectionStrength = 0.0f,
        ShadowSamples = 0,
        EnableGi = false,
        GiSamples = 0,
        GiStrength = 0.0f,
        EnableCaustics = false
    };

    public static RenderSettings Quality => new()
    {
        MaxBounces = 5,
        EnableShadows = true,
        EnableReflections = true,
        ReflectionStrength = 0.5f,
        ShadowSamples = 12,
        EnableGi = true,
        GiSamples = 8,
        GiStrength = 0.5f,
        EnableCaustics = true
    };
}