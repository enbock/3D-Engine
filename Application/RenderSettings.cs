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
    public int ResolutionScale { get; private init; } = 1;
    public int GiResolutionScale { get; private init; } = 1;
    public int ShadowResolutionScale { get; private init; } = 1;

    public static RenderSettings Default => new()
    {
        MaxBounces = 3,
        EnableShadows = true,
        EnableReflections = true,
        ReflectionStrength = 0.5f,
        ShadowSamples = 6,
        EnableGi = true,
        GiSamples = 2,
        GiStrength = 1f,
        EnableCaustics = true,
        ResolutionScale = 32,
        GiResolutionScale = 32,
        ShadowResolutionScale = 2
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
        GiStrength = 1f,
        EnableCaustics = true,
        ResolutionScale = 64,
        GiResolutionScale = 64,
        ShadowResolutionScale = 4
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
        EnableCaustics = false,
        ResolutionScale = 1,
        GiResolutionScale = 1,
        ShadowResolutionScale = 1
    };

    public static RenderSettings Quality => new()
    {
        MaxBounces = 5,
        EnableShadows = true,
        EnableReflections = true,
        ReflectionStrength = 0.5f,
        ShadowSamples = 8,
        EnableGi = true,
        GiSamples = 8,
        GiStrength = 1f,
        EnableCaustics = true,
        ResolutionScale = 16,
        GiResolutionScale = 4,
        ShadowResolutionScale = 2
    };

    public static RenderSettings UltraQuality => new()
    {
        MaxBounces = 8,
        EnableShadows = true,
        EnableReflections = true,
        ReflectionStrength = 0.5f,
        ShadowSamples = 12,
        EnableGi = true,
        GiSamples = 8,
        GiStrength = 1f,
        EnableCaustics = true,
        ResolutionScale = 16,
        GiResolutionScale = 4,
        ShadowResolutionScale = 1
    };
}