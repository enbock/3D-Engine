using System.Numerics;
using System.Runtime.InteropServices;
using Application;
using Application.Window;
using Core.Scene;
using Infrastructure.Vulkan.Helpers;
using Infrastructure.Vulkan.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Buffer = Silk.NET.Vulkan.Buffer;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Infrastructure.Vulkan;

public unsafe class InternalVulkanRenderer(WindowManager windowManager, EngineConfig config)
    : IDisposable
{
    private bool _buffersCreated;
    private VulkanBufferTask _bufferTask = null!;

    private Buffer _cameraBuffer;
    private DeviceMemory _cameraBufferMemory;
    private VulkanCommandTask _commandTask = null!;
    private Pipeline _computePipeline;
    private ShaderModule _computeShaderModule;

    private uint _currentFrame;
    private DescriptorPool _descriptorPool;
    private DescriptorSet _descriptorSet;

    private DescriptorSetLayout _descriptorSetLayout;

    private VulkanDeviceTask _deviceTask = null!;
    private VulkanImageTask _imageTask = null!;

    private Instance _instance;
    private bool _isInitialized;
    private KhrSurface _khrSurface = null!;
    private Buffer _lightBuffer;
    private DeviceMemory _lightBufferMemory;
    private VulkanMultiPassTask? _multiPassTask;
    private PipelineLayout _pipelineLayout;
    private VulkanPipelineTask _pipelineTask = null!;
    private Buffer _settingsBuffer;
    private DeviceMemory _settingsBufferMemory;

    private Image _storageImage;
    private DeviceMemory _storageImageMemory;
    private ImageView _storageImageView;
    private SurfaceKHR _surface;
    private VulkanSwapchainTask _swapchainTask = null!;
    private VulkanSyncTask _syncTask = null!;
    private float _time;
    private Buffer _triangleBuffer;
    private DeviceMemory _triangleBufferMemory;
    private Vk _vk = null!;

    public void Dispose()
    {
        if (!_isInitialized) return;

        _vk.DeviceWaitIdle(_deviceTask.Device);

        _syncTask.Dispose();
        _commandTask.Dispose();

        if (EngineConfig.UseMultiPassRendering)
        {
            _multiPassTask!.DestroyDescriptorPool();
            _multiPassTask.DestroyPipelines();
            _multiPassTask.DestroyGBufferImages();
        }
        else
        {
            _vk.DestroyDescriptorPool(_deviceTask.Device, _descriptorPool, null);
            _pipelineTask.DestroyPipeline(_computePipeline, _pipelineLayout, _computeShaderModule,
                _descriptorSetLayout);
        }

        _bufferTask.DestroyBuffer(_cameraBuffer, _cameraBufferMemory);
        _bufferTask.DestroyBuffer(_lightBuffer, _lightBufferMemory);
        _bufferTask.DestroyBuffer(_triangleBuffer, _triangleBufferMemory);
        _bufferTask.DestroyBuffer(_settingsBuffer, _settingsBufferMemory);

        _imageTask.DestroyImage(_storageImage, _storageImageView, _storageImageMemory);
        _swapchainTask.Dispose();
        _deviceTask.Dispose();

        _khrSurface.DestroySurface(_instance, _surface, null);
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }

    public void Initialize()
    {
        _vk = Vk.GetApi();

        CreateInstance();
        CreateSurface();

        _deviceTask = new VulkanDeviceTask(_vk, _khrSurface, _surface);
        _deviceTask.SelectPhysicalDevice();
        _deviceTask.CreateLogicalDevice(_instance);

        _swapchainTask = new VulkanSwapchainTask(
            _vk, _khrSurface, _deviceTask.KhrSwapchain,
            _deviceTask.PhysicalDevice, _deviceTask.Device, _surface, config);
        _swapchainTask.CreateSwapchain();

        _commandTask = new VulkanCommandTask(_vk, _deviceTask.Device, _deviceTask.QueueFamilyIndex);
        _commandTask.CreateCommandPool();

        _bufferTask = new VulkanBufferTask(_vk, _deviceTask.Device, _deviceTask);

        _imageTask = new VulkanImageTask(_vk, _deviceTask.Device, _deviceTask, _commandTask.CommandPool,
            _deviceTask.ComputeQueue);
        CreateStorageImage();

        _pipelineTask = new VulkanPipelineTask(_vk, _deviceTask.Device);

        if (EngineConfig.UseMultiPassRendering)
        {
            _multiPassTask = new VulkanMultiPassTask(_vk, _deviceTask.Device, _deviceTask, _imageTask, _pipelineTask);
            _multiPassTask.CreateGBufferImages(_swapchainTask.SwapchainExtent.Width,
                _swapchainTask.SwapchainExtent.Height);
            Console.WriteLine("Multi-Pass Rendering enabled");
        }
        else
        {
            CreatePipeline();
            Console.WriteLine("Single-Pass Rendering enabled");
        }

        _syncTask = new VulkanSyncTask(_vk, _deviceTask.Device);
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
        _imageTask.CreateImage(
            _swapchainTask.SwapchainExtent.Width,
            _swapchainTask.SwapchainExtent.Height,
            Format.R8G8B8A8Unorm,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.TransferSrcBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out _storageImage,
            out _storageImageMemory);

        _storageImageView = _imageTask.CreateImageView(_storageImage, Format.R8G8B8A8Unorm, ImageAspectFlags.ColorBit);
        _imageTask.TransitionImageLayout(_storageImage, ImageLayout.Undefined, ImageLayout.General);
    }

    private void CreatePipeline()
    {
        VulkanDescriptorHelper.CreateDescriptorSetLayoutForRaytracing(_pipelineTask, out _descriptorSetLayout);

        byte[] shaderCode = LoadShaderCode("Infrastructure/Vulkan/Shaders/raytracing.comp.spv");
        _computeShaderModule = _pipelineTask.CreateShaderModule(shaderCode);

        _pipelineLayout = _pipelineTask.CreatePipelineLayout(_descriptorSetLayout);
        _computePipeline = _pipelineTask.CreateComputePipeline(_computeShaderModule, _pipelineLayout);
    }

    private byte[] LoadShaderCode(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException($"Shader file not found: {path}");

        byte[] code = File.ReadAllBytes(path);
        Console.WriteLine($"Loaded shader: {path} ({code.Length} bytes)");
        return code;
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
        Result result = _deviceTask.KhrSwapchain.AcquireNextImage(
            _deviceTask.Device, _swapchainTask.Swapchain, ulong.MaxValue,
            _syncTask.ImageAvailableSemaphores[_currentFrame], default, &imageIndex);

        if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr) return;
        if (result != Result.Success) throw new Exception("Failed to acquire swapchain image");

        UpdateUniformBuffers(scene);

        CommandBuffer commandBuffer = _commandTask.CommandBuffers[_currentFrame];
        _vk.ResetCommandBuffer(commandBuffer, 0);

        if (EngineConfig.UseMultiPassRendering)
            _commandTask.RecordMultiPassCommands(
                commandBuffer,
                _multiPassTask!,
                _swapchainTask.SwapchainExtent,
                _storageImage,
                _swapchainTask.SwapchainImages[imageIndex]);
        else
            _commandTask.RecordComputeAndCopyCommands(
                commandBuffer,
                _computePipeline,
                _pipelineLayout,
                _descriptorSet,
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

        if (_vk.QueueSubmit(_deviceTask.ComputeQueue, 1, &submitInfo, _syncTask.InFlightFences[_currentFrame]) !=
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

        _deviceTask.KhrSwapchain.QueuePresent(_deviceTask.PresentQueue, &presentInfo);

        _currentFrame = (_currentFrame + 1) % (uint)EngineConfig.MaxFramesInFlight;
    }

    private void CreateBuffers(SceneEntity scene)
    {
        VulkanBufferHelper.CreateAndFillSceneBuffers(
            _bufferTask, scene,
            out _cameraBuffer, out _cameraBufferMemory,
            out _triangleBuffer, out _triangleBufferMemory,
            out _lightBuffer, out _lightBufferMemory,
            out _settingsBuffer, out _settingsBufferMemory);

        if (EngineConfig.UseMultiPassRendering)
        {
            _multiPassTask!.CreatePipelines(_cameraBuffer, _triangleBuffer, _lightBuffer, _settingsBuffer);
            _multiPassTask.CreateDescriptorSets(_cameraBuffer, _triangleBuffer, _lightBuffer, _settingsBuffer,
                _storageImageView);
        }
        else
        {
            VulkanDescriptorHelper.CreateDescriptorPoolAndSet(
                _pipelineTask, _descriptorSetLayout,
                out _descriptorPool, out _descriptorSet);

            VulkanDescriptorHelper.UpdateDescriptorSets(
                _pipelineTask, _descriptorSet, _storageImageView,
                _cameraBuffer, _triangleBuffer, _lightBuffer, _settingsBuffer);
        }
    }


    private void UpdateUniformBuffers(SceneEntity scene)
    {
        Vector2 resolution = new(_swapchainTask.SwapchainExtent.Width, _swapchainTask.SwapchainExtent.Height);
        VulkanBufferHelper.UpdateSceneBuffers(
            _bufferTask, scene,
            _cameraBufferMemory, _lightBufferMemory, _settingsBufferMemory,
            resolution, _time);
    }

    public void Resize(int width, int height)
    {
        if (!_isInitialized) return;

        _vk.DeviceWaitIdle(_deviceTask.Device);

        _swapchainTask.Cleanup();
        config.Width = width;
        config.Height = height;
        _swapchainTask.CreateSwapchain();

        _imageTask.DestroyImage(_storageImage, _storageImageView, _storageImageMemory);
        CreateStorageImage();

        if (EngineConfig.UseMultiPassRendering)
        {
            _multiPassTask!.DestroyGBufferImages();
            _multiPassTask.CreateGBufferImages(_swapchainTask.SwapchainExtent.Width,
                _swapchainTask.SwapchainExtent.Height);
            _multiPassTask.CreateDescriptorSets(_cameraBuffer, _triangleBuffer, _lightBuffer, _settingsBuffer,
                _storageImageView);
        }
        else
        {
            VulkanDescriptorHelper.UpdateDescriptorSets(
                _pipelineTask, _descriptorSet, _storageImageView,
                _cameraBuffer, _triangleBuffer, _lightBuffer, _settingsBuffer);
        }

        Console.WriteLine($"Window resized to {width}x{height}");
    }
}