namespace Application;

public class EngineConfig
{
    public EngineConfig()
    {
    }

    public EngineConfig(string title, int width, int height)
    {
        Title = title;
        Width = width;
        Height = height;
    }

    public string Title { get; set; } = "Vulkan Raytracing Engine";
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public bool VSync { get; set; } = true;
    public bool EnableValidation { get; set; } = true;
    public int MaxFramesInFlight { get; set; } = 2;
    public bool UseMultiPassRendering { get; set; } = true;
}