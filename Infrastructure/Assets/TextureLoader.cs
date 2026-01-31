using Infrastructure.Vulkan.Tasks;
using Silk.NET.Vulkan;
using StbImageSharp;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Infrastructure.Assets;

public unsafe class TextureLoader : ITextureLoader, IDisposable
{
    private readonly VulkanBufferTask _bufferTask;
    private readonly Device _device;
    private readonly VulkanDeviceTask _deviceTask;
    private readonly VulkanImageTask _imageTask;
    private readonly Dictionary<int, Texture> _textures = new();
    private readonly Vk _vk;
    private bool _disposed;
    private int _nextTextureId;

    public TextureLoader(
        Vk vk,
        Device device,
        VulkanDeviceTask deviceTask,
        VulkanImageTask imageTask,
        VulkanBufferTask bufferTask)
    {
        _vk = vk;
        _device = device;
        _deviceTask = deviceTask;
        _imageTask = imageTask;
        _bufferTask = bufferTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DisposeAll();
    }

    public TextureHandle LoadTexture(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Texture file not found: {filePath}");
            return CreateFallbackTexture(Path.GetFileName(filePath));
        }

        byte[] data = File.ReadAllBytes(filePath);
        return LoadTextureFromBytes(data, Path.GetFileName(filePath));
    }

    public TextureHandle LoadTextureFromBytes(byte[] data, string name)
    {
        try
        {
            ImageResult image = ImageResult.FromMemory(data, ColorComponents.RedGreenBlueAlpha);
            return UploadTexture(image.Data, image.Width, image.Height, name, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load texture {name}: {ex.Message}");
            return CreateFallbackTexture(name);
        }
    }

    public void DisposeTexture(TextureHandle handle)
    {
        if (_textures.TryGetValue(handle.Id, out Texture? texture))
        {
            texture.Dispose();
            _textures.Remove(handle.Id);
        }
    }

    public void DisposeAll()
    {
        foreach (Texture texture in _textures.Values)
        {
            texture.Dispose();
        }

        _textures.Clear();
    }

    public TextureHandle LoadNormalMap(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Normal map file not found: {filePath}");
            return CreateDefaultNormalMap(Path.GetFileName(filePath));
        }

        byte[] data = File.ReadAllBytes(filePath);
        ImageResult image = ImageResult.FromMemory(data, ColorComponents.RedGreenBlueAlpha);
        return UploadTexture(image.Data, image.Width, image.Height, Path.GetFileName(filePath), false);
    }

    public TextureHandle LoadNormalMapFromBytes(byte[] data, string name)
    {
        try
        {
            ImageResult image = ImageResult.FromMemory(data, ColorComponents.RedGreenBlueAlpha);
            return UploadTexture(image.Data, image.Width, image.Height, name, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load normal map {name}: {ex.Message}");
            return CreateDefaultNormalMap(name);
        }
    }

    private TextureHandle UploadTexture(byte[] pixelData, int width, int height, string name, bool isSrgb)
    {
        Format format = isSrgb ? Format.R8G8B8A8Srgb : Format.R8G8B8A8Unorm;
        ulong imageSize = (ulong)(width * height * 4);

        _bufferTask.CreateBuffer(
            imageSize,
            BufferUsageFlags.TransferSrcBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out Buffer stagingBuffer,
            out DeviceMemory stagingMemory);

        void* mappedData;
        _vk.MapMemory(_device, stagingMemory, 0, imageSize, 0, &mappedData);
        fixed (byte* dataPtr = pixelData)
        {
            System.Buffer.MemoryCopy(dataPtr, mappedData, imageSize, imageSize);
        }

        _vk.UnmapMemory(_device, stagingMemory);

        _imageTask.CreateImage(
            (uint)width,
            (uint)height,
            format,
            ImageTiling.Optimal,
            ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out Image textureImage,
            out DeviceMemory textureMemory);

        TransitionImageLayout(textureImage, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
        CopyBufferToImage(stagingBuffer, textureImage, (uint)width, (uint)height);
        TransitionImageLayout(textureImage, ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);

        _bufferTask.DestroyBuffer(stagingBuffer, stagingMemory);

        ImageView imageView = _imageTask.CreateImageView(textureImage, format, ImageAspectFlags.ColorBit);
        Sampler sampler = CreateSampler();

        int textureId = _nextTextureId++;
        Texture texture = new(_vk, _device)
        {
            Id = textureId,
            Name = name,
            Width = width,
            Height = height,
            Image = textureImage,
            ImageView = imageView,
            Memory = textureMemory,
            Sampler = sampler,
            Format = format
        };

        _textures[textureId] = texture;

        Console.WriteLine($"Texture loaded: {name} ({width}x{height}) ID={textureId}");
        return texture.ToHandle();
    }

    private TextureHandle CreateFallbackTexture(string name)
    {
        byte[] magenta = [255, 0, 255, 255, 0, 0, 0, 255, 0, 0, 0, 255, 255, 0, 255, 255];
        return UploadTexture(magenta, 2, 2, $"fallback_{name}", true);
    }

    private TextureHandle CreateDefaultNormalMap(string name)
    {
        byte[] defaultNormal = [128, 128, 255, 255];
        return UploadTexture(defaultNormal, 1, 1, $"default_normal_{name}", false);
    }

    private void TransitionImageLayout(Image image, ImageLayout oldLayout, ImageLayout newLayout)
    {
        CommandBuffer cmdBuffer = BeginSingleTimeCommands();

        ImageMemoryBarrier barrier = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = oldLayout,
            NewLayout = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = image,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            }
        };

        PipelineStageFlags sourceStage;
        PipelineStageFlags destinationStage;

        if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
        {
            barrier.SrcAccessMask = 0;
            barrier.DstAccessMask = AccessFlags.TransferWriteBit;
            sourceStage = PipelineStageFlags.TopOfPipeBit;
            destinationStage = PipelineStageFlags.TransferBit;
        }
        else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
            barrier.DstAccessMask = AccessFlags.ShaderReadBit;
            sourceStage = PipelineStageFlags.TransferBit;
            destinationStage = PipelineStageFlags.FragmentShaderBit;
        }
        else
        {
            throw new Exception($"Unsupported layout transition: {oldLayout} -> {newLayout}");
        }

        _vk.CmdPipelineBarrier(cmdBuffer, sourceStage, destinationStage, 0, 0, null, 0, null, 1, &barrier);

        EndSingleTimeCommands(cmdBuffer);
    }

    private void CopyBufferToImage(Buffer buffer, Image image, uint width, uint height)
    {
        CommandBuffer cmdBuffer = BeginSingleTimeCommands();

        BufferImageCopy region = new()
        {
            BufferOffset = 0,
            BufferRowLength = 0,
            BufferImageHeight = 0,
            ImageSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            ImageOffset = new Offset3D(0, 0, 0),
            ImageExtent = new Extent3D(width, height, 1)
        };

        _vk.CmdCopyBufferToImage(cmdBuffer, buffer, image, ImageLayout.TransferDstOptimal, 1, &region);

        EndSingleTimeCommands(cmdBuffer);
    }

    private Sampler CreateSampler()
    {
        PhysicalDeviceProperties properties;
        _vk.GetPhysicalDeviceProperties(_deviceTask.PhysicalDevice, &properties);

        SamplerCreateInfo samplerInfo = new()
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = Filter.Linear,
            MinFilter = Filter.Linear,
            AddressModeU = SamplerAddressMode.Repeat,
            AddressModeV = SamplerAddressMode.Repeat,
            AddressModeW = SamplerAddressMode.Repeat,
            AnisotropyEnable = Vk.True,
            MaxAnisotropy = properties.Limits.MaxSamplerAnisotropy,
            BorderColor = BorderColor.IntOpaqueBlack,
            UnnormalizedCoordinates = Vk.False,
            CompareEnable = Vk.False,
            CompareOp = CompareOp.Always,
            MipmapMode = SamplerMipmapMode.Linear,
            MipLodBias = 0,
            MinLod = 0,
            MaxLod = 0
        };

        if (_vk.CreateSampler(_device, &samplerInfo, null, out Sampler sampler) != Result.Success)
            throw new Exception("Failed to create texture sampler");

        return sampler;
    }

    private CommandBuffer BeginSingleTimeCommands()
    {
        CommandBufferAllocateInfo allocInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            Level = CommandBufferLevel.Primary,
            CommandPool = _deviceTask.TransferCommandPool,
            CommandBufferCount = 1
        };

        CommandBuffer commandBuffer;
        _vk.AllocateCommandBuffers(_device, &allocInfo, &commandBuffer);

        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        _vk.BeginCommandBuffer(commandBuffer, &beginInfo);

        return commandBuffer;
    }

    private void EndSingleTimeCommands(CommandBuffer commandBuffer)
    {
        _vk.EndCommandBuffer(commandBuffer);

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer
        };

        _vk.QueueSubmit(_deviceTask.GraphicsQueue, 1, &submitInfo, default);
        _vk.QueueWaitIdle(_deviceTask.GraphicsQueue);

        _vk.FreeCommandBuffers(_device, _deviceTask.TransferCommandPool, 1, &commandBuffer);
    }

    public Texture? GetTexture(int id)
    {
        return _textures.GetValueOrDefault(id);
    }
}