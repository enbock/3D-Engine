namespace VulkanEngine.Application;

public class RenderSettings
{
    public int MaxBounces { get; set; } = 3;
    public bool EnableShadows { get; set; } = true;
    public bool EnableReflections { get; set; } = true;
    public float ReflectionStrength { get; set; } = 0.5f;
    public int ShadowSamples { get; set; } = 4;
    public float ShadowSoftness { get; set; } = 0.05f;

    public static RenderSettings Default => new()
    {
        MaxBounces = 3,
        EnableShadows = true,
        EnableReflections = true,
        ReflectionStrength = 0.5f,
        ShadowSamples = 4,
        ShadowSoftness = 0.05f
    };

    public static RenderSettings Performance => new()
    {
        MaxBounces = 1,
        EnableShadows = true,
        EnableReflections = true,
        ReflectionStrength = 0.3f,
        ShadowSamples = 1,
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
