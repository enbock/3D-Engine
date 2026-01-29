using Application;
using Silk.NET.Vulkan;
using VkSemaphore = Silk.NET.Vulkan.Semaphore;

namespace Infrastructure.Vulkan.Tasks;

public unsafe class VulkanSyncTask(Vk vk, Device device, EngineConfig config)
{
    public VkSemaphore[] ImageAvailableSemaphores { get; private set; } = Array.Empty<VkSemaphore>();
    public VkSemaphore[] RenderFinishedSemaphores { get; private set; } = Array.Empty<VkSemaphore>();
    public Fence[] InFlightFences { get; private set; } = Array.Empty<Fence>();

    public void CreateSyncObjects(uint swapchainImageCount)
    {
        ImageAvailableSemaphores = new VkSemaphore[config.MaxFramesInFlight];
        RenderFinishedSemaphores = new VkSemaphore[swapchainImageCount];
        InFlightFences = new Fence[config.MaxFramesInFlight];

        SemaphoreCreateInfo semaphoreInfo = new()
        {
            SType = StructureType.SemaphoreCreateInfo
        };

        FenceCreateInfo fenceInfo = new()
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };

        for (int i = 0; i < config.MaxFramesInFlight; i++)
        {
            VkSemaphore semaphore;
            if (vk.CreateSemaphore(device, &semaphoreInfo, null, &semaphore) != Result.Success)
                throw new Exception("Failed to create image available semaphore");

            ImageAvailableSemaphores[i] = semaphore;

            Fence fence;
            if (vk.CreateFence(device, &fenceInfo, null, &fence) != Result.Success)
                throw new Exception("Failed to create fence");

            InFlightFences[i] = fence;
        }

        for (int i = 0; i < swapchainImageCount; i++)
        {
            VkSemaphore semaphore;
            if (vk.CreateSemaphore(device, &semaphoreInfo, null, &semaphore) != Result.Success)
                throw new Exception("Failed to create render finished semaphore");

            RenderFinishedSemaphores[i] = semaphore;
        }
    }

    public void WaitForFence(uint frameIndex)
    {
        fixed (Fence* fencePtr = &InFlightFences[frameIndex])
        {
            vk.WaitForFences(device, 1, fencePtr, true, ulong.MaxValue);
            vk.ResetFences(device, 1, fencePtr);
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < config.MaxFramesInFlight; i++)
        {
            vk.DestroySemaphore(device, ImageAvailableSemaphores[i], null);
            vk.DestroyFence(device, InFlightFences[i], null);
        }

        foreach (VkSemaphore t in RenderFinishedSemaphores) vk.DestroySemaphore(device, t, null);
    }
}