using Silk.NET.Vulkan;

namespace Infrastructure.Assets;

public class Texture : IDisposable
{
    private readonly Device _device;

    private readonly Vk _vk;
    private bool _disposed;

    public Texture(Vk vk, Device device)
    {
        _vk = vk;
        _device = device;
    }

    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public Image Image { get; init; }
    public ImageView ImageView { get; init; }
    public DeviceMemory Memory { get; init; }
    public Sampler Sampler { get; init; }
    public Format Format { get; init; }

    public unsafe void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _vk.DestroySampler(_device, Sampler, null);
        _vk.DestroyImageView(_device, ImageView, null);
        _vk.DestroyImage(_device, Image, null);
        _vk.FreeMemory(_device, Memory, null);
    }

    public TextureHandle ToHandle()
    {
        return new TextureHandle
        {
            Id = Id,
            Name = Name,
            Width = Width,
            Height = Height
        };
    }
}