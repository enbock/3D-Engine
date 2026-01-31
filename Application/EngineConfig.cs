namespace Application;

public class EngineConfig
{
    public string Title { get; init; } = "Vulkan Raytracing Engine";
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public bool VSync { get; init; } = true;
    public bool EnableValidation { get; init; } = true;
    public bool EnableHdr10 { get; init; } = true;
    public float HdrMinNits { get; init; } = 0.0f;
    public float HdrMaxNits { get; init; } = 400.0f;
    public float Exposure { get; init; } = 1.0f;
    public float Gamma { get; init; } = 2.2f;
    public ToneMappingOperator ToneMapping { get; init; } = ToneMappingOperator.AcesFilmic;
    public static int MaxFramesInFlight => 2;
    public static bool UseMultiPassRendering => true;
}