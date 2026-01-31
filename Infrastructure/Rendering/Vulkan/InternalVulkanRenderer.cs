using System.Numerics;
using System.Runtime.InteropServices;
using Application;
using Application.Window;
using Core.Scene;
using Infrastructure.Rendering.Vulkan.Helpers;
using Infrastructure.Rendering.Vulkan.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Buffer = Silk.NET.Vulkan.Buffer;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using InfraTextureLoader = Infrastructure.Assets.TextureLoader;

namespace Infrastructure.Rendering.Vulkan;

public unsafe class InternalVulkanRenderer(WindowManager windowManager, EngineConfig config)
    : IDisposable
{
    private bool _buffersCreated;
    private Buffer _bvhBuffer;
    private DeviceMemory _bvhBufferMemory;

    private Buffer _cameraBuffer;
    private DeviceMemory _cameraBufferMemory;
    private VulkanCommandTask _commandTask = null!;

    private uint _currentFrame;

    private Instance _instance;
    private bool _isInitialized;
    private KhrSurface _khrSurface = null!;
    private Buffer _lightBuffer;
    private DeviceMemory _lightBufferMemory;
    private VulkanMultiPassTask? _multiPassTask;
    private VulkanPipelineTask _pipelineTask = null!;
    private Buffer _settingsBuffer;
    private DeviceMemory _settingsBufferMemory;

    private Image _storageImage;
    private DeviceMemory _storageImageMemory;
    private ImageView _storageImageView;
    private SurfaceKHR _surface;
    private VulkanSwapchainTask _swapchainTask = null!;
    private VulkanSyncTask _syncTask = null!;
    private InfraTextureLoader? _textureLoader;
    private float _time;
    private Buffer _triangleBuffer;
    private DeviceMemory _triangleBufferMemory;
    private Vk _vk = null!;

    public InfraTextureLoader TextureLoader => _textureLoader ?? throw new InvalidOperationException("Renderer not initialized");
    public VulkanBufferTask BufferTask { get; private set; } = null!;

    public VulkanDeviceTask DeviceTask { get; private set; } = null!;

    public VulkanImageTask ImageTask { get; private set; } = null!;

    public void Dispose()
    {
        if (!_isInitialized) return;

        _vk.DeviceWaitIdle(DeviceTask.Device);

        _textureLoader?.Dispose();
        _syncTask.Dispose();
        _commandTask.Dispose();

        _multiPassTask!.DestroyDescriptorPool();
        _multiPassTask.DestroyPipelines();
        _multiPassTask.DestroyGBufferImages();


        BufferTask.DestroyBuffer(_cameraBuffer, _cameraBufferMemory);
        BufferTask.DestroyBuffer(_lightBuffer, _lightBufferMemory);
        BufferTask.DestroyBuffer(_triangleBuffer, _triangleBufferMemory);
        BufferTask.DestroyBuffer(_settingsBuffer, _settingsBufferMemory);
        BufferTask.DestroyBuffer(_bvhBuffer, _bvhBufferMemory);

        ImageTask.DestroyImage(_storageImage, _storageImageView, _storageImageMemory);
        _swapchainTask.Dispose();
        DeviceTask.Dispose();

        _khrSurface.DestroySurface(_instance, _surface, null);
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }

    public void Initialize()
    {
        _vk = Vk.GetApi();

        CreateInstance();
        CreateSurface();

        DeviceTask = new VulkanDeviceTask(_vk, _khrSurface, _surface);
        DeviceTask.SelectPhysicalDevice();
        DeviceTask.CreateLogicalDevice(_instance);

        _swapchainTask = new VulkanSwapchainTask(
            _vk, _khrSurface, DeviceTask.KhrSwapchain,
            DeviceTask.PhysicalDevice, DeviceTask.Device, _surface, config);
        _swapchainTask.CreateSwapchain();

        _commandTask = new VulkanCommandTask(_vk, DeviceTask.Device, DeviceTask.QueueFamilyIndex);
        _commandTask.CreateCommandPool();

        BufferTask = new VulkanBufferTask(_vk, DeviceTask.Device, DeviceTask);

        ImageTask = new VulkanImageTask(_vk, DeviceTask.Device, DeviceTask, _commandTask.CommandPool,
            DeviceTask.ComputeQueue);
        CreateStorageImage();

        _textureLoader = new InfraTextureLoader(_vk, DeviceTask.Device, DeviceTask, ImageTask, BufferTask);

        _pipelineTask = new VulkanPipelineTask(_vk, DeviceTask.Device);

        _multiPassTask = new VulkanMultiPassTask(_vk, DeviceTask.Device, ImageTask, _pipelineTask);
        _multiPassTask.CreateGBufferImages(
            _swapchainTask.SwapchainExtent.Width,
            _swapchainTask.SwapchainExtent.Height,
            config.RenderSettings.GiResolutionScale,
            config.RenderSettings.ShadowResolutionScale);
        Console.WriteLine("Multi-Pass Rendering enabled");

        _syncTask = new VulkanSyncTask(_vk, DeviceTask.Device);
        _syncTask.CreateSyncObjects((uint)_swapchainTask.SwapchainImages.Length);

        _isInitialized = true;
        Console.WriteLine("Vulkan Renderer fully initialized");
    }

    private void CreateInstance()
    {
        ApplicationInfo appInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)Marshal.StringToHGlobalAnsi(config.Title),
            ApplicationVersion = Vk.MakeVersion(1, 0),
            PEngineName = (byte*)Marshal.StringToHGlobalAnsi("VulkanEngine"),
            EngineVersion = Vk.MakeVersion(1, 0),
            ApiVersion = Vk.Version12
        };

        string[] extensions = windowManager.GetRequiredExtensions();
        IntPtr extensionNames = SilkMarshal.StringArrayToPtr(extensions);

        InstanceCreateInfo createInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo,
            EnabledExtensionCount = (uint)extensions.Length,
            PpEnabledExtensionNames = (byte**)extensionNames
        };

        if (config.EnableValidation)
        {
            string[] layers = ["VK_LAYER_KHRONOS_validation"];
            IntPtr layerNames = SilkMarshal.StringArrayToPtr(layers);
            createInfo.EnabledLayerCount = 1;
            createInfo.PpEnabledLayerNames = (byte**)layerNames;
        }

        if (_vk.CreateInstance(&createInfo, null, out _instance) != Result.Success)
            throw new Exception("Failed to create Vulkan instance");

        if (config.EnableValidation) SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);

        SilkMarshal.Free(extensionNames);
        Marshal.FreeHGlobal((nint)appInfo.PApplicationName);
        Marshal.FreeHGlobal((nint)appInfo.PEngineName);
    }

    private void CreateSurface()
    {
        _surface = windowManager.CreateVulkanSurface(_instance);

        if (!_vk.TryGetInstanceExtension(_instance, out _khrSurface))
            throw new Exception("KHR_surface extension not available");
    }

    private void CreateStorageImage()
    {
        Format storageFormat = Format.R16G16B16A16Sfloat;

        ImageTask.CreateImage(
            _swapchainTask.SwapchainExtent.Width,
            _swapchainTask.SwapchainExtent.Height,
            storageFormat,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.TransferSrcBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out _storageImage,
            out _storageImageMemory);

        _storageImageView = ImageTask.CreateImageView(_storageImage, storageFormat, ImageAspectFlags.ColorBit);
        ImageTask.TransitionImageLayout(_storageImage, ImageLayout.Undefined, ImageLayout.General);
    }

    public void Render(SceneEntity scene, float deltaTime)
    {
        if (!_isInitialized) return;

        if (!_buffersCreated)
        {
            Console.WriteLine($"Creating buffers for {scene.Triangles.Count} triangles, {scene.Lights.Count} lights");
            CreateBuffers(scene);
            _buffersCreated = true;
            Console.WriteLine("Buffers created successfully");
        }

        _time += deltaTime;

        _syncTask.WaitForFence(_currentFrame);

        uint imageIndex;
        Result result = DeviceTask.KhrSwapchain.AcquireNextImage(
            DeviceTask.Device, _swapchainTask.Swapchain, ulong.MaxValue,
            _syncTask.ImageAvailableSemaphores[_currentFrame], default, &imageIndex);

        if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr) return;
        if (result != Result.Success) throw new Exception("Failed to acquire swapchain image");

        UpdateUniformBuffers(scene);

        CommandBuffer commandBuffer = _commandTask.CommandBuffers[_currentFrame];
        _vk.ResetCommandBuffer(commandBuffer, 0);

        _commandTask.RecordMultiPassCommands(
            commandBuffer,
            _multiPassTask!,
            _swapchainTask.SwapchainExtent,
            _storageImage,
            _swapchainTask.SwapchainImages[imageIndex]);

        Semaphore waitSemaphore = _syncTask.ImageAvailableSemaphores[_currentFrame];
        Semaphore signalSemaphore = _syncTask.RenderFinishedSemaphores[imageIndex];
        PipelineStageFlags waitStages = PipelineStageFlags.ComputeShaderBit;

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = &waitSemaphore,
            PWaitDstStageMask = &waitStages,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer,
            SignalSemaphoreCount = 1,
            PSignalSemaphores = &signalSemaphore
        };

        if (_vk.QueueSubmit(DeviceTask.ComputeQueue, 1, &submitInfo, _syncTask.InFlightFences[_currentFrame]) !=
            Result.Success) throw new Exception("Failed to submit command buffer");

        SwapchainKHR swapchains = _swapchainTask.Swapchain;
        PresentInfoKHR presentInfo = new()
        {
            SType = StructureType.PresentInfoKhr,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = &signalSemaphore,
            SwapchainCount = 1,
            PSwapchains = &swapchains,
            PImageIndices = &imageIndex
        };

        DeviceTask.KhrSwapchain.QueuePresent(DeviceTask.PresentQueue, &presentInfo);

        _currentFrame = (_currentFrame + 1) % (uint)EngineConfig.MaxFramesInFlight;
    }

    private void CreateBuffers(SceneEntity scene)
    {
        VulkanBufferHelper.CreateAndFillSceneBuffers(
            BufferTask, scene,
            out _cameraBuffer, out _cameraBufferMemory,
            out _triangleBuffer, out _triangleBufferMemory,
            out _lightBuffer, out _lightBufferMemory,
            out _settingsBuffer, out _settingsBufferMemory,
            out _bvhBuffer, out _bvhBufferMemory);

        _multiPassTask!.CreatePipelines();
        _multiPassTask.CreateDescriptorSets(_cameraBuffer, _triangleBuffer, _lightBuffer, _settingsBuffer, _bvhBuffer,
            _storageImageView);
    }


    private void UpdateUniformBuffers(SceneEntity scene)
    {
        Vector2 resolution = new(_swapchainTask.SwapchainExtent.Width, _swapchainTask.SwapchainExtent.Height);
        VulkanBufferHelper.UpdateSceneBuffers(
            BufferTask, scene, config,
            _cameraBufferMemory, _lightBufferMemory, _settingsBufferMemory,
            resolution, _time);
    }

    public void Resize(int width, int height)
    {
        if (!_isInitialized) return;

        _vk.DeviceWaitIdle(DeviceTask.Device);

        _swapchainTask.Cleanup();
        config.Width = width;
        config.Height = height;
        _swapchainTask.CreateSwapchain();

        ImageTask.DestroyImage(_storageImage, _storageImageView, _storageImageMemory);
        CreateStorageImage();

        _multiPassTask!.DestroyGBufferImages();
        _multiPassTask.CreateGBufferImages(_swapchainTask.SwapchainExtent.Width,
            _swapchainTask.SwapchainExtent.Height,
            config.RenderSettings.GiResolutionScale,
            config.RenderSettings.ShadowResolutionScale);
        _multiPassTask.CreateDescriptorSets(_cameraBuffer, _triangleBuffer, _lightBuffer, _settingsBuffer, _bvhBuffer,
            _storageImageView);

        Console.WriteLine($"Window resized to {width}x{height}");
    }
}