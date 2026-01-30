using System.Runtime.InteropServices;
using Silk.NET.Input;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace Application.Window;

public unsafe class WindowManager(IWindow window)
{
    private IInputContext? inputContext;
    private bool isDisposed;

    public event Action<int, int>? OnResize;
    public event Action<float>? OnUpdate;
    public event Action<float>? OnRender;
    public event Action? OnLoad;

    public void Initialize()
    {
        window.Load += () =>
        {
            inputContext = window.CreateInput();
            OnLoad?.Invoke();
        };

        window.Update += deltaTime => { OnUpdate?.Invoke((float)deltaTime); };
        window.Render += deltaTime => { OnRender?.Invoke((float)deltaTime); };
        window.Resize += size => { OnResize?.Invoke(size.X, size.Y); };
    }

    public void Run()
    {
        window.Run();
    }

    public void Close()
    {
        window.Close();
    }

    public string[] GetRequiredExtensions()
    {
        if (window.VkSurface == null) return Array.Empty<string>();

        byte** extensionsPtr = window.VkSurface.GetRequiredExtensions(out uint count);

        if (extensionsPtr == null || count == 0) return Array.Empty<string>();

        string[] extensions = new string[count];
        for (int i = 0; i < count; i++)
        {
            string? ptr = Marshal.PtrToStringAnsi((IntPtr)extensionsPtr[i]);
            extensions[i] = ptr ?? string.Empty;
        }

        return extensions;
    }

    public SurfaceKHR CreateVulkanSurface(Instance instance)
    {
        return window.VkSurface == null ? throw new InvalidOperationException("Vulkan surface not available") : window.VkSurface.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface();
    }

    public void Dispose()
    {
        if (isDisposed) return;

        inputContext?.Dispose();
        window.Dispose();

        isDisposed = true;
    }

    public IInputContext GetInputContext()
    {
        return inputContext!;
    }
}