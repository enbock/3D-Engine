using Silk.NET.Windowing;
using Silk.NET.Input;
using VulkanEngine.Application;
using Silk.NET.Vulkan;
using System.Runtime.InteropServices;

namespace VulkanEngine.Infrastructure.Window;

public unsafe class WindowManager : IDisposable
{
    private IWindow? _window;
    private readonly EngineConfig _config;
    private bool _isDisposed;

    public IWindow Window => _window ?? throw new InvalidOperationException("Window not initialized");
    public IInputContext? InputContext { get; private set; }

    public event Action<int, int>? OnResize;
    public event Action<float>? OnUpdate;
    public event Action<float>? OnRender;
    public event Action? OnLoad;

    public WindowManager(EngineConfig config)
    {
        _config = config;
    }

    public void Initialize()
    {
        var options = WindowOptions.Default;
        options.Title = _config.Title;
        options.Size = new Silk.NET.Maths.Vector2D<int>(_config.Width, _config.Height);
        options.VSync = _config.VSync;
        options.API = GraphicsAPI.DefaultVulkan;

        _window = Silk.NET.Windowing.Window.Create(options);

        _window.Load += OnLoadInternal;
        _window.Update += OnUpdateInternal;
        _window.Render += OnRenderInternal;
        _window.Resize += OnResizeInternal;
        _window.Closing += OnClosing;
    }

    private void OnLoadInternal()
    {
        InputContext = _window!.CreateInput();
        OnLoad?.Invoke();
    }

    private void OnUpdateInternal(double deltaTime)
    {
        OnUpdate?.Invoke((float)deltaTime);
    }

    private void OnRenderInternal(double deltaTime)
    {
        OnRender?.Invoke((float)deltaTime);
    }

    private void OnResizeInternal(Silk.NET.Maths.Vector2D<int> size)
    {
        OnResize?.Invoke(size.X, size.Y);
    }

    private void OnClosing()
    {
    }

    public void Run()
    {
        _window?.Run();
    }

    public void Close()
    {
        _window?.Close();
    }

    public string[] GetRequiredExtensions()
    {
        if (_window?.VkSurface == null) return Array.Empty<string>();
        
        uint count = 0;
        byte** extensionsPtr = _window.VkSurface.GetRequiredExtensions(out count);
        if (extensionsPtr == null) return Array.Empty<string>();
        
        var extensions = new string[count];
        for (uint i = 0; i < count; i++)
        {
            extensions[i] = Marshal.PtrToStringAnsi((nint)extensionsPtr[i]) ?? string.Empty;
        }
        return extensions;
    }

    public unsafe SurfaceKHR CreateVulkanSurface(Instance instance, Vk vk)
    {
        return _window?.VkSurface?.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface() 
            ?? throw new Exception("Failed to create Vulkan surface");
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        InputContext = null;

        try
        {
            _window?.Dispose();
        }
        catch { }

        _window = null;
    }
}
