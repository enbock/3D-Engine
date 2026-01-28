using Silk.NET.Input;
using Silk.NET.Vulkan;

namespace Application.Window;

public interface WindowManager : IDisposable
{
    IInputContext? InputContext { get; }
    event Action? OnLoad;
    event Action<float>? OnUpdate;
    event Action<float>? OnRender;
    event Action<int, int>? OnResize;
    
    void Initialize();
    void Run();
    void Close();
    string[] GetRequiredExtensions();
    SurfaceKHR CreateVulkanSurface(Instance instance, Vk vk);
}
