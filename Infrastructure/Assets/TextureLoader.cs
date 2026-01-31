using Infrastructure.Rendering.Vulkan.Tasks;
using Silk.NET.Vulkan;
using StbImageSharp;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Infrastructure.Assets;

public unsafe class TextureLoader(
    Vk vk,
    Device device,
    VulkanDeviceTask deviceTask,
    VulkanImageTask imageTask,
    VulkanBufferTask bufferTask)
    : Core.Assets.TextureLoader
{
    private readonly Dictionary<int, Texture> _textures = new();
    private bool _disposed;
    private int _nextTextureId;

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

    public Texture? GetTexture(int id)
    {
        return _textures.GetValueOrDefault(id);
    }

    private TextureHandle UploadTexture(byte[] pixelData, int width, int height, string name, bool isSrgb)
    {
        Format format = isSrgb ? Format.R8G8B8A8Srgb : Format.R8G8B8A8Unorm;
        ulong imageSize = (ulong)(width * height * 4);

        bufferTask.CreateBuffer(
            imageSize,
            BufferUsageFlags.TransferSrcBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out Buffer stagingBuffer,
            out DeviceMemory stagingMemory);

        void* mappedData;
        vk.MapMemory(device, stagingMemory, 0, imageSize, 0, &mappedData);
        fixed (byte* dataPtr = pixelData)
        {
            System.Buffer.MemoryCopy(dataPtr, mappedData, imageSize, imageSize);
        }

        vk.UnmapMemory(device, stagingMemory);

        imageTask.CreateImage(
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

        bufferTask.DestroyBuffer(stagingBuffer, stagingMemory);

        ImageView imageView = imageTask.CreateImageView(textureImage, format, ImageAspectFlags.ColorBit);
        Sampler sampler = CreateSampler();

        int textureId = _nextTextureId++;
        Texture texture = new(vk, device)
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

        vk.CmdPipelineBarrier(cmdBuffer, sourceStage, destinationStage, 0, 0, null, 0, null, 1, &barrier);

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

        vk.CmdCopyBufferToImage(cmdBuffer, buffer, image, ImageLayout.TransferDstOptimal, 1, &region);

        EndSingleTimeCommands(cmdBuffer);
    }

    private Sampler CreateSampler()
    {
        PhysicalDeviceProperties properties;
        vk.GetPhysicalDeviceProperties(deviceTask.PhysicalDevice, &properties);

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

        if (vk.CreateSampler(device, &samplerInfo, null, out Sampler sampler) != Result.Success)
            throw new Exception("Failed to create texture sampler");

        return sampler;
    }

    private CommandBuffer BeginSingleTimeCommands()
    {
        CommandBufferAllocateInfo allocInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            Level = CommandBufferLevel.Primary,
            CommandPool = deviceTask.TransferCommandPool,
            CommandBufferCount = 1
        };

        CommandBuffer commandBuffer;
        vk.AllocateCommandBuffers(device, &allocInfo, &commandBuffer);

        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        vk.BeginCommandBuffer(commandBuffer, &beginInfo);

        return commandBuffer;
    }

    private void EndSingleTimeCommands(CommandBuffer commandBuffer)
    {
        vk.EndCommandBuffer(commandBuffer);

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer
        };

        vk.QueueSubmit(deviceTask.GraphicsQueue, 1, &submitInfo, default);
        vk.QueueWaitIdle(deviceTask.GraphicsQueue);

        vk.FreeCommandBuffers(device, deviceTask.TransferCommandPool, 1, &commandBuffer);
    }
}