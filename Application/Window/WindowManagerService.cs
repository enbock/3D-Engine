using System.Runtime.InteropServices;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace Application.Window;

public unsafe class WindowManagerService : WindowManager
{
    private readonly EngineConfig config;
    private bool isDisposed;
    private IWindow? window;

    public WindowManagerService(EngineConfig config)
    {
        this.config = config;
    }

    public IWindow Window => window ?? throw new InvalidOperationException("Window not initialized");
    public IInputContext? InputContext { get; private set; }

    public event Action<int, int>? OnResize;
    public event Action<float>? OnUpdate;
    public event Action<float>? OnRender;
    public event Action? OnLoad;

    public void Initialize()
    {
        WindowOptions options = WindowOptions.DefaultVulkan with
        {
            Title = config.Title,
            Size = new Vector2D<int>(config.Width, config.Height),
            VSync = config.VSync,
            API = new GraphicsAPI(ContextAPI.Vulkan, ContextProfile.Core, ContextFlags.Default, new APIVersion(1, 0))
        };

        window = Silk.NET.Windowing.Window.Create(options);

        window.Load += () =>
        {
            InputContext = window.CreateInput();
            OnLoad?.Invoke();
        };

        window.Update += deltaTime => { OnUpdate?.Invoke((float)deltaTime); };

        window.Render += deltaTime => { OnRender?.Invoke((float)deltaTime); };

        window.Resize += size => { OnResize?.Invoke(size.X, size.Y); };
    }

    public void Run()
    {
        window?.Run();
    }

    public void Close()
    {
        window?.Close();
    }

    public string[] GetRequiredExtensions()
    {
        if (window?.VkSurface == null) return Array.Empty<string>();

        uint count = 0;
        byte** extensionsPtr = window.VkSurface.GetRequiredExtensions(out count);

        if (extensionsPtr == null || count == 0) return Array.Empty<string>();

        string[] extensions = new string[count];
        for (int i = 0; i < count; i++)
        {
            string? ptr = Marshal.PtrToStringAnsi((IntPtr)extensionsPtr[i]);
            extensions[i] = ptr ?? string.Empty;
        }

        return extensions;
    }

    public SurfaceKHR CreateVulkanSurface(Instance instance, Vk vk)
    {
        if (window?.VkSurface == null) throw new InvalidOperationException("Vulkan surface not available");

        return window.VkSurface.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface();
    }

    public void Dispose()
    {
        if (isDisposed) return;

        InputContext?.Dispose();
        window?.Dispose();

        isDisposed = true;
    }
}