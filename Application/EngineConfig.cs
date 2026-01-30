namespace Application;

public class EngineConfig
{
    public string Title { get; init; } = "Vulkan Raytracing Engine";
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public bool VSync { get; init; } = true;
    public bool EnableValidation { get; init; } = true;
    public static int MaxFramesInFlight => 1;
    public static bool UseMultiPassRendering => true;
}