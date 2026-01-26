namespace VulkanEngine.Application;

public class EngineConfig
{
    public string Title { get; set; } = "Vulkan Raytracing Engine";
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 720;
    public bool VSync { get; set; } = true;
    public bool EnableValidation { get; set; } = true;
    public int MaxFramesInFlight { get; set; } = 2;

    public EngineConfig()
    {
    }

    public EngineConfig(string title, int width, int height)
    {
        Title = title;
        Width = width;
        Height = height;
    }
}
