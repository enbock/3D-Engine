using Application;
using Silk.NET.Vulkan;

namespace Infrastructure.Vulkan.Tasks;

public unsafe class VulkanCommandTask(Vk vk, Device device, uint queueFamilyIndex)
{
    public CommandPool CommandPool { get; private set; }
    public CommandBuffer[] CommandBuffers { get; private set; } = Array.Empty<CommandBuffer>();

    public void CreateCommandPool()
    {
        CommandPoolCreateInfo poolInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = queueFamilyIndex,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };

        if (vk.CreateCommandPool(device, &poolInfo, null, out CommandPool commandPool) != Result.Success)
            throw new Exception("Failed to create command pool");

        CommandPool = commandPool;

        CommandBuffers = new CommandBuffer[EngineConfig.MaxFramesInFlight];
        CommandBufferAllocateInfo allocInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = CommandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = (uint)EngineConfig.MaxFramesInFlight
        };

        fixed (CommandBuffer* commandBuffersPtr = CommandBuffers)
        {
            if (vk.AllocateCommandBuffers(device, &allocInfo, commandBuffersPtr) != Result.Success)
                throw new Exception("Failed to allocate command buffers");
        }
    }

    public void RecordComputeAndCopyCommands(
        CommandBuffer commandBuffer,
        Pipeline pipeline,
        PipelineLayout pipelineLayout,
        DescriptorSet descriptorSet,
        Extent2D extent,
        Image storageImage,
        Image swapchainImage)
    {
        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo
        };

        if (vk.BeginCommandBuffer(commandBuffer, &beginInfo) != Result.Success)
            throw new Exception("Failed to begin recording command buffer");

        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pipeline);
        vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pipelineLayout, 0, 1, &descriptorSet, 0,
            null);

        uint groupCountX = (extent.Width + 15) / 16;
        uint groupCountY = (extent.Height + 15) / 16;
        vk.CmdDispatch(commandBuffer, groupCountX, groupCountY, 1);

        ImageMemoryBarrier barrier1 = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = ImageLayout.General,
            NewLayout = ImageLayout.TransferSrcOptimal,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = storageImage,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            SrcAccessMask = AccessFlags.ShaderWriteBit,
            DstAccessMask = AccessFlags.TransferReadBit
        };

        vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.ComputeShaderBit,
            PipelineStageFlags.TransferBit, 0, 0, null, 0, null, 1, &barrier1);

        ImageMemoryBarrier barrier2 = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = ImageLayout.Undefined,
            NewLayout = ImageLayout.TransferDstOptimal,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = swapchainImage,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            SrcAccessMask = 0,
            DstAccessMask = AccessFlags.TransferWriteBit
        };

        vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TopOfPipeBit,
            PipelineStageFlags.TransferBit, 0, 0, null, 0, null, 1, &barrier2);

        ImageCopy copyRegion = new()
        {
            SrcSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            SrcOffset = new Offset3D(0, 0, 0),
            DstSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            DstOffset = new Offset3D(0, 0, 0),
            Extent = new Extent3D(extent.Width, extent.Height, 1)
        };

        vk.CmdCopyImage(commandBuffer, storageImage, ImageLayout.TransferSrcOptimal,
            swapchainImage, ImageLayout.TransferDstOptimal, 1, &copyRegion);

        ImageMemoryBarrier barrier3 = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = ImageLayout.TransferDstOptimal,
            NewLayout = ImageLayout.PresentSrcKhr,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = swapchainImage,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            SrcAccessMask = AccessFlags.TransferWriteBit,
            DstAccessMask = 0
        };

        vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit,
            PipelineStageFlags.BottomOfPipeBit, 0, 0, null, 0, null, 1, &barrier3);

        ImageMemoryBarrier barrier4 = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = ImageLayout.TransferSrcOptimal,
            NewLayout = ImageLayout.General,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = storageImage,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            SrcAccessMask = AccessFlags.TransferReadBit,
            DstAccessMask = AccessFlags.ShaderWriteBit
        };

        vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit,
            PipelineStageFlags.ComputeShaderBit, 0, 0, null, 0, null, 1, &barrier4);

        if (vk.EndCommandBuffer(commandBuffer) != Result.Success)
            throw new Exception("Failed to record command buffer");
    }

    public void RecordMultiPassCommands(
        CommandBuffer commandBuffer,
        VulkanMultiPassTask multiPassTask,
        Extent2D extent,
        Image storageImage,
        Image swapchainImage)
    {
        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo
        };

        if (vk.BeginCommandBuffer(commandBuffer, &beginInfo) != Result.Success)
            throw new Exception("Failed to begin recording command buffer");

        multiPassTask.RecordMultiPassCommands(commandBuffer, extent);

        ImageMemoryBarrier barrier1 = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = ImageLayout.General,
            NewLayout = ImageLayout.TransferSrcOptimal,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = storageImage,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            SrcAccessMask = AccessFlags.ShaderWriteBit,
            DstAccessMask = AccessFlags.TransferReadBit
        };

        vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.ComputeShaderBit,
            PipelineStageFlags.TransferBit, 0, 0, null, 0, null, 1, &barrier1);

        ImageMemoryBarrier barrier2 = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = ImageLayout.Undefined,
            NewLayout = ImageLayout.TransferDstOptimal,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = swapchainImage,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            SrcAccessMask = 0,
            DstAccessMask = AccessFlags.TransferWriteBit
        };

        vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TopOfPipeBit,
            PipelineStageFlags.TransferBit, 0, 0, null, 0, null, 1, &barrier2);

        ImageCopy copyRegion = new()
        {
            SrcSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            SrcOffset = new Offset3D(0, 0, 0),
            DstSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            DstOffset = new Offset3D(0, 0, 0),
            Extent = new Extent3D(extent.Width, extent.Height, 1)
        };

        vk.CmdCopyImage(commandBuffer, storageImage, ImageLayout.TransferSrcOptimal,
            swapchainImage, ImageLayout.TransferDstOptimal, 1, &copyRegion);

        ImageMemoryBarrier barrier3 = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = ImageLayout.TransferDstOptimal,
            NewLayout = ImageLayout.PresentSrcKhr,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = swapchainImage,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            SrcAccessMask = AccessFlags.TransferWriteBit,
            DstAccessMask = 0
        };

        vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit,
            PipelineStageFlags.BottomOfPipeBit, 0, 0, null, 0, null, 1, &barrier3);

        ImageMemoryBarrier barrier4 = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = ImageLayout.TransferSrcOptimal,
            NewLayout = ImageLayout.General,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = storageImage,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            SrcAccessMask = AccessFlags.TransferReadBit,
            DstAccessMask = AccessFlags.ShaderWriteBit
        };

        vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit,
            PipelineStageFlags.ComputeShaderBit, 0, 0, null, 0, null, 1, &barrier4);

        if (vk.EndCommandBuffer(commandBuffer) != Result.Success)
            throw new Exception("Failed to record command buffer");
    }

    public void Dispose()
    {
        vk.DestroyCommandPool(device, CommandPool, null);
    }
}