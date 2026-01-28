using System.Numerics;
using System.Runtime.InteropServices;
using Application;
using Application.Window;
using Core.Math;
using Core.Scene;
using Core.Scene.Geometry;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Buffer = Silk.NET.Vulkan.Buffer;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using Vector3 = System.Numerics.Vector3;

namespace Infrastructure.Vulkan;

public unsafe class InternalVulkanRenderer : IDisposable
{
    private readonly EngineConfig _config;
    private readonly WindowManagerService _windowManager;

    private bool _buffersCreated;

    private Buffer _cameraBuffer;
    private DeviceMemory _cameraBufferMemory;
    private CommandBuffer[] _commandBuffers = Array.Empty<CommandBuffer>();

    private CommandPool _commandPool;
    private Pipeline _computePipeline;
    private Queue _computeQueue;
    private ShaderModule _computeShaderModule;

    private uint _currentFrame;

    private DescriptorPool _descriptorPool;
    private DescriptorSet _descriptorSet;

    private DescriptorSetLayout _descriptorSetLayout;
    private Device _device;

    private Semaphore[] _imageAvailableSemaphores = Array.Empty<Semaphore>();
    private Fence[] _inFlightFences = Array.Empty<Fence>();

    private Instance _instance;
    private bool _isInitialized;

    private KhrSurface? _khrSurface;
    private KhrSwapchain? _khrSwapchain;
    private Buffer _lightBuffer;
    private DeviceMemory _lightBufferMemory;
    private PhysicalDevice _physicalDevice;
    private PipelineLayout _pipelineLayout;
    private Queue _presentQueue;
    private uint _queueFamilyIndex;
    private Semaphore[] _renderFinishedSemaphores = Array.Empty<Semaphore>();
    private Buffer _settingsBuffer;
    private DeviceMemory _settingsBufferMemory;

    private Image _storageImage;
    private DeviceMemory _storageImageMemory;
    private ImageView _storageImageView;
    private SurfaceKHR _surface;
    private SwapchainKHR _swapchain;
    private Extent2D _swapchainExtent;
    private Format _swapchainFormat;
    private Image[] _swapchainImages = Array.Empty<Image>();
    private ImageView[] _swapchainImageViews = Array.Empty<ImageView>();
    private float _time;
    private Buffer _triangleBuffer;
    private DeviceMemory _triangleBufferMemory;
    private Vk _vk = null!;

    public InternalVulkanRenderer(WindowManagerService windowManager, EngineConfig config)
    {
        _windowManager = windowManager;
        _config = config;
    }

    public void Dispose()
    {
        if (!_isInitialized) return;

        _vk.DeviceWaitIdle(_device);

        for (int i = 0; i < _config.MaxFramesInFlight; i++)
        {
            _vk.DestroySemaphore(_device, _imageAvailableSemaphores[i], null);
            _vk.DestroyFence(_device, _inFlightFences[i], null);
        }

        for (int i = 0; i < _renderFinishedSemaphores.Length; i++)
            _vk.DestroySemaphore(_device, _renderFinishedSemaphores[i], null);

        _vk.DestroyCommandPool(_device, _commandPool, null);

        _vk.DestroyDescriptorPool(_device, _descriptorPool, null);

        _vk.DestroyBuffer(_device, _cameraBuffer, null);
        _vk.FreeMemory(_device, _cameraBufferMemory, null);
        _vk.DestroyBuffer(_device, _lightBuffer, null);
        _vk.FreeMemory(_device, _lightBufferMemory, null);
        _vk.DestroyBuffer(_device, _triangleBuffer, null);
        _vk.FreeMemory(_device, _triangleBufferMemory, null);
        _vk.DestroyBuffer(_device, _settingsBuffer, null);
        _vk.FreeMemory(_device, _settingsBufferMemory, null);

        _vk.DestroyPipeline(_device, _computePipeline, null);
        _vk.DestroyPipelineLayout(_device, _pipelineLayout, null);
        _vk.DestroyDescriptorSetLayout(_device, _descriptorSetLayout, null);
        _vk.DestroyShaderModule(_device, _computeShaderModule, null);

        _vk.DestroyImageView(_device, _storageImageView, null);
        _vk.DestroyImage(_device, _storageImage, null);
        _vk.FreeMemory(_device, _storageImageMemory, null);

        CleanupSwapchain();

        _vk.DestroyDevice(_device, null);
        _khrSurface!.DestroySurface(_instance, _surface, null);
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }

    public void Initialize()
    {
        _vk = Vk.GetApi();

        CreateInstance();
        CreateSurface();
        SelectPhysicalDevice();
        CreateLogicalDevice();
        CreateSwapchain();
        CreateCommandPool();
        CreateStorageImage();
        CreateDescriptorSetLayout();
        CreateComputePipeline();
        CreateSyncObjects();

        _isInitialized = true;
        Console.WriteLine("Vulkan Pipeline fully initialized");
    }

    private void CreateInstance()
    {
        var appInfo = new ApplicationInfo
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)Marshal.StringToHGlobalAnsi(_config.Title),
            ApplicationVersion = Vk.MakeVersion(1, 0, 0),
            PEngineName = (byte*)Marshal.StringToHGlobalAnsi("VulkanEngine"),
            EngineVersion = Vk.MakeVersion(1, 0, 0),
            ApiVersion = Vk.Version12
        };

        var extensions = _windowManager.GetRequiredExtensions();
        var extensionNames = SilkMarshal.StringArrayToPtr(extensions);

        var createInfo = new InstanceCreateInfo
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo,
            EnabledExtensionCount = (uint)extensions.Length,
            PpEnabledExtensionNames = (byte**)extensionNames
        };

        if (_config.EnableValidation)
        {
            var layers = new[] { "VK_LAYER_KHRONOS_validation" };
            var layerNames = SilkMarshal.StringArrayToPtr(layers);
            createInfo.EnabledLayerCount = 1;
            createInfo.PpEnabledLayerNames = (byte**)layerNames;
        }

        if (_vk.CreateInstance(&createInfo, null, out _instance) != Result.Success)
        {
            throw new Exception("Failed to create Vulkan instance");
        }

        if (_config.EnableValidation)
        {
            SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
        }

        SilkMarshal.Free((nint)extensionNames);
        Marshal.FreeHGlobal((nint)appInfo.PApplicationName);
        Marshal.FreeHGlobal((nint)appInfo.PEngineName);
    }

    private void CreateSurface()
    {
        _surface = _windowManager.CreateVulkanSurface(_instance, _vk);

        if (!_vk.TryGetInstanceExtension(_instance, out _khrSurface))
        {
            throw new Exception("KHR_surface extension not available");
        }
    }

    private void SelectPhysicalDevice()
    {
        uint deviceCount = 0;
        _vk.EnumeratePhysicalDevices(_instance, &deviceCount, null);

        if (deviceCount == 0)
        {
            throw new Exception("No Vulkan-capable GPU found");
        }

        var devices = stackalloc PhysicalDevice[(int)deviceCount];
        _vk.EnumeratePhysicalDevices(_instance, &deviceCount, devices);

        for (int i = 0; i < deviceCount; i++)
        {
            var device = devices[i];
            if (IsDeviceSuitable(device))
            {
                _physicalDevice = device;
                PhysicalDeviceProperties props;
                _vk.GetPhysicalDeviceProperties(device, &props);
                var deviceName = Marshal.PtrToStringAnsi((nint)props.DeviceName);
                Console.WriteLine($"Selected GPU: {deviceName}");
                return;
            }
        }

        throw new Exception("No suitable GPU found");
    }

    private bool IsDeviceSuitable(PhysicalDevice device)
    {
        uint queueFamilyCount = 0;
        _vk.GetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, null);

        var queueFamilies = stackalloc QueueFamilyProperties[(int)queueFamilyCount];
        _vk.GetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, queueFamilies);

        for (uint i = 0; i < queueFamilyCount; i++)
        {
            var hasCompute = (queueFamilies[i].QueueFlags & QueueFlags.ComputeBit) != 0;

            Bool32 presentSupport = false;
            _khrSurface!.GetPhysicalDeviceSurfaceSupport(device, i, _surface, &presentSupport);

            if (hasCompute && presentSupport)
            {
                _queueFamilyIndex = i;
                return true;
            }
        }

        return false;
    }

    private void CreateLogicalDevice()
    {
        float queuePriority = 1.0f;
        var queueCreateInfo = new DeviceQueueCreateInfo
        {
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = _queueFamilyIndex,
            QueueCount = 1,
            PQueuePriorities = &queuePriority
        };

        var deviceFeatures = new PhysicalDeviceFeatures();

        var extensions = new[] { "VK_KHR_swapchain" };
        var extensionNames = SilkMarshal.StringArrayToPtr(extensions);

        var createInfo = new DeviceCreateInfo
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = 1,
            PQueueCreateInfos = &queueCreateInfo,
            PEnabledFeatures = &deviceFeatures,
            EnabledExtensionCount = (uint)extensions.Length,
            PpEnabledExtensionNames = (byte**)extensionNames
        };

        if (_vk.CreateDevice(_physicalDevice, &createInfo, null, out _device) != Result.Success)
        {
            throw new Exception("Failed to create logical device");
        }

        _vk.GetDeviceQueue(_device, _queueFamilyIndex, 0, out _computeQueue);
        _presentQueue = _computeQueue;

        SilkMarshal.Free((nint)extensionNames);

        if (!_vk.TryGetDeviceExtension(_instance, _device, out _khrSwapchain))
        {
            throw new Exception("KHR_swapchain extension not available");
        }
    }

    private void CreateSwapchain()
    {
        SurfaceCapabilitiesKHR capabilities;
        _khrSurface!.GetPhysicalDeviceSurfaceCapabilities(_physicalDevice, _surface, &capabilities);

        uint formatCount;
        _khrSurface.GetPhysicalDeviceSurfaceFormats(_physicalDevice, _surface, &formatCount, null);
        var formats = stackalloc SurfaceFormatKHR[(int)formatCount];
        _khrSurface.GetPhysicalDeviceSurfaceFormats(_physicalDevice, _surface, &formatCount, formats);

        _swapchainFormat = formats[0].Format;
        var colorSpace = formats[0].ColorSpace;

        for (int i = 0; i < formatCount; i++)
        {
            if (formats[i].Format == Format.B8G8R8A8Srgb &&
                formats[i].ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
            {
                _swapchainFormat = formats[i].Format;
                colorSpace = formats[i].ColorSpace;
                break;
            }
        }

        _swapchainExtent = new Extent2D
        {
            Width = (uint)_config.Width,
            Height = (uint)_config.Height
        };

        var imageCount = capabilities.MinImageCount + 1;
        if (capabilities.MaxImageCount > 0 && imageCount > capabilities.MaxImageCount)
        {
            imageCount = capabilities.MaxImageCount;
        }

        var createInfo = new SwapchainCreateInfoKHR
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _surface,
            MinImageCount = imageCount,
            ImageFormat = _swapchainFormat,
            ImageColorSpace = colorSpace,
            ImageExtent = _swapchainExtent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.TransferDstBit | ImageUsageFlags.ColorAttachmentBit,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform = capabilities.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = _config.VSync ? PresentModeKHR.FifoKhr : PresentModeKHR.ImmediateKhr,
            Clipped = true
        };

        if (_khrSwapchain!.CreateSwapchain(_device, &createInfo, null, out _swapchain) != Result.Success)
        {
            throw new Exception("Failed to create swapchain");
        }

        uint swapchainImageCount;
        _khrSwapchain.GetSwapchainImages(_device, _swapchain, &swapchainImageCount, null);
        _swapchainImages = new Image[swapchainImageCount];
        fixed (Image* imagesPtr = _swapchainImages)
        {
            _khrSwapchain.GetSwapchainImages(_device, _swapchain, &swapchainImageCount, imagesPtr);
        }

        _swapchainImageViews = new ImageView[swapchainImageCount];
        for (int i = 0; i < swapchainImageCount; i++)
        {
            var viewInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = _swapchainImages[i],
                ViewType = ImageViewType.Type2D,
                Format = _swapchainFormat,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                }
            };

            if (_vk.CreateImageView(_device, &viewInfo, null, out _swapchainImageViews[i]) != Result.Success)
            {
                throw new Exception("Failed to create image view");
            }
        }
    }

    private void CreateStorageImage()
    {
        var imageInfo = new ImageCreateInfo
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Format = Format.R8G8B8A8Unorm,
            Extent = new Extent3D(_swapchainExtent.Width, _swapchainExtent.Height, 1),
            MipLevels = 1,
            ArrayLayers = 1,
            Samples = SampleCountFlags.Count1Bit,
            Tiling = ImageTiling.Optimal,
            Usage = ImageUsageFlags.StorageBit | ImageUsageFlags.TransferSrcBit,
            SharingMode = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Undefined
        };

        if (_vk.CreateImage(_device, &imageInfo, null, out _storageImage) != Result.Success)
        {
            throw new Exception("Failed to create storage image");
        }

        MemoryRequirements memRequirements;
        _vk.GetImageMemoryRequirements(_device, _storageImage, &memRequirements);

        var allocInfo = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = FindMemoryType(memRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit)
        };

        if (_vk.AllocateMemory(_device, &allocInfo, null, out _storageImageMemory) != Result.Success)
        {
            throw new Exception("Failed to allocate storage image memory");
        }

        _vk.BindImageMemory(_device, _storageImage, _storageImageMemory, 0);

        var viewInfo = new ImageViewCreateInfo
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = _storageImage,
            ViewType = ImageViewType.Type2D,
            Format = Format.R8G8B8A8Unorm,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            }
        };

        if (_vk.CreateImageView(_device, &viewInfo, null, out _storageImageView) != Result.Success)
        {
            throw new Exception("Failed to create storage image view");
        }

        TransitionImageLayout(_storageImage, ImageLayout.Undefined, ImageLayout.General);
    }

    private uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties)
    {
        PhysicalDeviceMemoryProperties memProperties;
        _vk.GetPhysicalDeviceMemoryProperties(_physicalDevice, &memProperties);

        for (uint i = 0; i < memProperties.MemoryTypeCount; i++)
        {
            if ((typeFilter & (1 << (int)i)) != 0 &&
                (memProperties.MemoryTypes[(int)i].PropertyFlags & properties) == properties)
            {
                return i;
            }
        }

        throw new Exception("Failed to find suitable memory type");
    }

    private void TransitionImageLayout(Image image, ImageLayout oldLayout, ImageLayout newLayout)
    {
        var cmdBuffer = BeginSingleTimeCommands();

        var barrier = new ImageMemoryBarrier
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

        if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.General)
        {
            barrier.SrcAccessMask = 0;
            barrier.DstAccessMask = AccessFlags.ShaderWriteBit;
            sourceStage = PipelineStageFlags.TopOfPipeBit;
            destinationStage = PipelineStageFlags.ComputeShaderBit;
        }
        else if (oldLayout == ImageLayout.General && newLayout == ImageLayout.TransferSrcOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.ShaderWriteBit;
            barrier.DstAccessMask = AccessFlags.TransferReadBit;
            sourceStage = PipelineStageFlags.ComputeShaderBit;
            destinationStage = PipelineStageFlags.TransferBit;
        }
        else if (oldLayout == ImageLayout.TransferSrcOptimal && newLayout == ImageLayout.General)
        {
            barrier.SrcAccessMask = AccessFlags.TransferReadBit;
            barrier.DstAccessMask = AccessFlags.ShaderWriteBit;
            sourceStage = PipelineStageFlags.TransferBit;
            destinationStage = PipelineStageFlags.ComputeShaderBit;
        }
        else
        {
            throw new Exception($"Unsupported layout transition: {oldLayout} -> {newLayout}");
        }

        _vk.CmdPipelineBarrier(cmdBuffer, sourceStage, destinationStage, 0, 0, null, 0, null, 1, &barrier);

        EndSingleTimeCommands(cmdBuffer);
    }

    private CommandBuffer BeginSingleTimeCommands()
    {
        var allocInfo = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            Level = CommandBufferLevel.Primary,
            CommandPool = _commandPool,
            CommandBufferCount = 1
        };

        CommandBuffer commandBuffer;
        _vk.AllocateCommandBuffers(_device, &allocInfo, &commandBuffer);

        var beginInfo = new CommandBufferBeginInfo
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

        var submitInfo = new SubmitInfo
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer
        };

        _vk.QueueSubmit(_computeQueue, 1, &submitInfo, default);
        _vk.QueueWaitIdle(_computeQueue);

        _vk.FreeCommandBuffers(_device, _commandPool, 1, &commandBuffer);
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

        fixed (Fence* fencesPtr = &_inFlightFences[_currentFrame])
        {
            _vk.WaitForFences(_device, 1, fencesPtr, true, ulong.MaxValue);
            _vk.ResetFences(_device, 1, fencesPtr);
        }

        uint imageIndex;
        var result = _khrSwapchain!.AcquireNextImage(_device, _swapchain, ulong.MaxValue,
            _imageAvailableSemaphores[_currentFrame], default, &imageIndex);

        if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr)
        {
            return;
        }
        else if (result != Result.Success)
        {
            throw new Exception("Failed to acquire swapchain image");
        }

        UpdateUniformBuffers(scene);

        var commandBuffer = _commandBuffers[_currentFrame];
        _vk.ResetCommandBuffer(commandBuffer, 0);
        RecordCommandBuffer(commandBuffer, imageIndex);

        var waitSemaphore = _imageAvailableSemaphores[_currentFrame];
        var signalSemaphore = _renderFinishedSemaphores[imageIndex];
        var waitStages = PipelineStageFlags.ComputeShaderBit;
        // ...existing code...

        var submitInfo = new SubmitInfo
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

        if (_vk.QueueSubmit(_computeQueue, 1, &submitInfo, _inFlightFences[_currentFrame]) != Result.Success)
        {
            throw new Exception("Failed to submit command buffer");
        }

        var swapchains = _swapchain;
        var presentInfo = new PresentInfoKHR
        {
            SType = StructureType.PresentInfoKhr,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = &signalSemaphore,
            SwapchainCount = 1,
            PSwapchains = &swapchains,
            PImageIndices = &imageIndex
        };

        _khrSwapchain.QueuePresent(_presentQueue, &presentInfo);

        _currentFrame = (_currentFrame + 1) % (uint)_config.MaxFramesInFlight;
    }

    private void UpdateUniformBuffers(SceneEntity scene)
    {
        var cameraData = new CameraUniformData
        {
            Position = new Vector3(
                scene.Camera.Position.X,
                scene.Camera.Position.Y,
                scene.Camera.Position.Z
            ),
            Target = new Vector3(
                scene.Camera.Target.X,
                scene.Camera.Target.Y,
                scene.Camera.Target.Z
            ),
            Resolution = new Vector2(_swapchainExtent.Width, _swapchainExtent.Height),
            Time = _time,
            Fov = scene.Camera.Fov
        };

        void* data;
        _vk.MapMemory(_device, _cameraBufferMemory, 0, (ulong)sizeof(CameraUniformData), 0, &data);
        *(CameraUniformData*)data = cameraData;
        _vk.UnmapMemory(_device, _cameraBufferMemory);

        var lights = scene.Lights.Take(8).ToArray();

        var lightData = new LightUniformData
        {
            NumLights = lights.Length
        };

        for (int i = 0; i < lights.Length; i++)
        {
            var lightEntry = new LightData
            {
                Type = (int)lights[i].Type,
                Intensity = lights[i].Intensity,
                PositionX = lights[i].Position.X,
                PositionY = lights[i].Position.Y,
                PositionZ = lights[i].Position.Z,
                DirectionX = lights[i].Direction.X,
                DirectionY = lights[i].Direction.Y,
                DirectionZ = lights[i].Direction.Z,
                ColorR = lights[i].Color.R,
                ColorG = lights[i].Color.G,
                ColorB = lights[i].Color.B
            };

            lightData.SetLight(i, lightEntry);
        }

        _vk.MapMemory(_device, _lightBufferMemory, 0, (ulong)sizeof(LightUniformData), 0, &data);
        *(LightUniformData*)data = lightData;
        _vk.UnmapMemory(_device, _lightBufferMemory);

        var settings = RenderSettings.Default;
        var settingsData = new RenderSettingsData
        {
            MaxBounces = settings.MaxBounces,
            EnableShadows = settings.EnableShadows ? 1 : 0,
            EnableReflections = settings.EnableReflections ? 1 : 0,
            ReflectionStrength = settings.ReflectionStrength,
            ShadowSamples = settings.ShadowSamples,
            ShadowSoftness = settings.ShadowSoftness,
            Pad = new Vector2(0, 0)
        };

        _vk.MapMemory(_device, _settingsBufferMemory, 0, (ulong)sizeof(RenderSettingsData), 0, &data);
        *(RenderSettingsData*)data = settingsData;
        _vk.UnmapMemory(_device, _settingsBufferMemory);
    }

    private void RecordCommandBuffer(CommandBuffer commandBuffer, uint imageIndex)
    {
        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo
        };

        if (_vk.BeginCommandBuffer(commandBuffer, &beginInfo) != Result.Success)
        {
            throw new Exception("Failed to begin recording command buffer");
        }

        _vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, _computePipeline);

        fixed (DescriptorSet* descriptorSetPtr = &_descriptorSet)
        {
            _vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, _pipelineLayout,
                0, 1, descriptorSetPtr, 0, null);
        }

        uint groupCountX = (_swapchainExtent.Width + 15) / 16;
        uint groupCountY = (_swapchainExtent.Height + 15) / 16;
        _vk.CmdDispatch(commandBuffer, groupCountX, groupCountY, 1);

        var barrier = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = ImageLayout.General,
            NewLayout = ImageLayout.TransferSrcOptimal,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = _storageImage,
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

        _vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.ComputeShaderBit,
            PipelineStageFlags.TransferBit, 0, 0, null, 0, null, 1, &barrier);

        var barrier2 = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = ImageLayout.Undefined,
            NewLayout = ImageLayout.TransferDstOptimal,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = _swapchainImages[imageIndex],
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

        _vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TopOfPipeBit,
            PipelineStageFlags.TransferBit, 0, 0, null, 0, null, 1, &barrier2);

        var copyRegion = new ImageCopy
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
            Extent = new Extent3D(_swapchainExtent.Width, _swapchainExtent.Height, 1)
        };

        _vk.CmdCopyImage(commandBuffer, _storageImage, ImageLayout.TransferSrcOptimal,
            _swapchainImages[imageIndex], ImageLayout.TransferDstOptimal, 1, &copyRegion);

        var barrier3 = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = ImageLayout.TransferDstOptimal,
            NewLayout = ImageLayout.PresentSrcKhr,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = _swapchainImages[imageIndex],
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

        _vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit,
            PipelineStageFlags.BottomOfPipeBit, 0, 0, null, 0, null, 1, &barrier3);

        var barrier4 = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = ImageLayout.TransferSrcOptimal,
            NewLayout = ImageLayout.General,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = _storageImage,
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

        _vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit,
            PipelineStageFlags.ComputeShaderBit, 0, 0, null, 0, null, 1, &barrier4);

        if (_vk.EndCommandBuffer(commandBuffer) != Result.Success)
        {
            throw new Exception("Failed to record command buffer");
        }
    }

    public void Resize(int width, int height)
    {
        if (!_isInitialized) return;

        _vk.DeviceWaitIdle(_device);

        CleanupSwapchain();

        _config.Width = width;
        _config.Height = height;

        CreateSwapchain();

        _vk.DestroyImageView(_device, _storageImageView, null);
        _vk.DestroyImage(_device, _storageImage, null);
        _vk.FreeMemory(_device, _storageImageMemory, null);

        CreateStorageImage();
        UpdateDescriptorSets();

        Console.WriteLine($"Window resized to {width}x{height}");
    }

    private void CleanupSwapchain()
    {
        foreach (var imageView in _swapchainImageViews)
        {
            _vk.DestroyImageView(_device, imageView, null);
        }

        _khrSwapchain!.DestroySwapchain(_device, _swapchain, null);
    }

    private void CreateDescriptorSetLayout()
    {
        var bindings = stackalloc DescriptorSetLayoutBinding[5];

        bindings[0] = new DescriptorSetLayoutBinding
        {
            Binding = 0,
            DescriptorType = DescriptorType.StorageImage,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.ComputeBit
        };

        bindings[1] = new DescriptorSetLayoutBinding
        {
            Binding = 1,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.ComputeBit
        };

        bindings[2] = new DescriptorSetLayoutBinding
        {
            Binding = 2,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.ComputeBit
        };

        bindings[3] = new DescriptorSetLayoutBinding
        {
            Binding = 3,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.ComputeBit
        };

        bindings[4] = new DescriptorSetLayoutBinding
        {
            Binding = 4,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.ComputeBit
        };

        var layoutInfo = new DescriptorSetLayoutCreateInfo
        {
            SType = StructureType.DescriptorSetLayoutCreateInfo,
            BindingCount = 5,
            PBindings = bindings
        };

        if (_vk.CreateDescriptorSetLayout(_device, &layoutInfo, null, out _descriptorSetLayout) != Result.Success)
        {
            throw new Exception("Failed to create descriptor set layout");
        }
    }

    private void CreateComputePipeline()
    {
        var shaderCode = LoadShaderCode("Infrastructure/Vulkan/Shaders/raytracing.comp.spv");
        _computeShaderModule = CreateShaderModule(shaderCode);

        var shaderStageInfo = new PipelineShaderStageCreateInfo
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.ComputeBit,
            Module = _computeShaderModule,
            PName = (byte*)Marshal.StringToHGlobalAnsi("main")
        };

        fixed (DescriptorSetLayout* layoutPtr = &_descriptorSetLayout)
        {
            var pipelineLayoutInfo = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 1,
                PSetLayouts = layoutPtr
            };

            if (_vk.CreatePipelineLayout(_device, &pipelineLayoutInfo, null, out _pipelineLayout) != Result.Success)
            {
                throw new Exception("Failed to create pipeline layout");
            }
        }

        var pipelineInfo = new ComputePipelineCreateInfo
        {
            SType = StructureType.ComputePipelineCreateInfo,
            Stage = shaderStageInfo,
            Layout = _pipelineLayout
        };

        if (_vk.CreateComputePipelines(_device, default, 1, &pipelineInfo, null, out _computePipeline) !=
            Result.Success)
        {
            throw new Exception("Failed to create compute pipeline");
        }

        Marshal.FreeHGlobal((nint)shaderStageInfo.PName);
    }

    private byte[] LoadShaderCode(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Shader file not found: {path}");
        }

        var code = File.ReadAllBytes(path);
        Console.WriteLine($"Loaded shader: {path} ({code.Length} bytes)");
        return code;
    }

    private ShaderModule CreateShaderModule(byte[] code)
    {
        fixed (byte* codePtr = code)
        {
            var createInfo = new ShaderModuleCreateInfo
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)code.Length,
                PCode = (uint*)codePtr
            };

            if (_vk.CreateShaderModule(_device, &createInfo, null, out var shaderModule) != Result.Success)
            {
                throw new Exception("Failed to create shader module");
            }

            return shaderModule;
        }
    }

    private void CreateCommandPool()
    {
        var poolInfo = new CommandPoolCreateInfo
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = _queueFamilyIndex,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };

        if (_vk.CreateCommandPool(_device, &poolInfo, null, out _commandPool) != Result.Success)
        {
            throw new Exception("Failed to create command pool");
        }

        _commandBuffers = new CommandBuffer[_config.MaxFramesInFlight];
        var allocInfo = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = (uint)_config.MaxFramesInFlight
        };

        fixed (CommandBuffer* commandBuffersPtr = _commandBuffers)
        {
            if (_vk.AllocateCommandBuffers(_device, &allocInfo, commandBuffersPtr) != Result.Success)
            {
                throw new Exception("Failed to allocate command buffers");
            }
        }
    }

    private void CreateSyncObjects()
    {
        uint swapchainImageCount = (uint)_swapchainImages.Length;

        _imageAvailableSemaphores = new Semaphore[_config.MaxFramesInFlight];
        _renderFinishedSemaphores = new Semaphore[swapchainImageCount];
        _inFlightFences = new Fence[_config.MaxFramesInFlight];

        var semaphoreInfo = new SemaphoreCreateInfo
        {
            SType = StructureType.SemaphoreCreateInfo
        };

        var fenceInfo = new FenceCreateInfo
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };

        for (int i = 0; i < _config.MaxFramesInFlight; i++)
        {
            if (_vk.CreateSemaphore(_device, &semaphoreInfo, null, out _imageAvailableSemaphores[i]) !=
                Result.Success ||
                _vk.CreateFence(_device, &fenceInfo, null, out _inFlightFences[i]) != Result.Success)
            {
                throw new Exception("Failed to create synchronization objects");
            }
        }

        for (int i = 0; i < swapchainImageCount; i++)
        {
            if (_vk.CreateSemaphore(_device, &semaphoreInfo, null, out _renderFinishedSemaphores[i]) != Result.Success)
            {
                throw new Exception("Failed to create render finished semaphore");
            }
        }
    }

    private void CreateBuffers(SceneEntity scene)
    {
        CreateCameraBuffer();
        CreateLightBuffer();
        CreateTriangleBuffer(scene);
        CreateSettingsBuffer();
        CreateDescriptorPool();
        CreateDescriptorSet();
        UpdateDescriptorSets();
    }

    private void CreateCameraBuffer()
    {
        ulong bufferSize = (ulong)sizeof(CameraUniformData);
        CreateBuffer(bufferSize, BufferUsageFlags.UniformBufferBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out _cameraBuffer, out _cameraBufferMemory);
    }

    private void CreateLightBuffer()
    {
        ulong bufferSize = (ulong)sizeof(LightUniformData);
        CreateBuffer(bufferSize, BufferUsageFlags.StorageBufferBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out _lightBuffer, out _lightBufferMemory);
    }

    private void CreateSettingsBuffer()
    {
        ulong bufferSize = (ulong)sizeof(RenderSettingsData);
        CreateBuffer(bufferSize, BufferUsageFlags.UniformBufferBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out _settingsBuffer, out _settingsBufferMemory);
    }

    private void CreateTriangleBuffer(SceneEntity scene)
    {
        var triangles = scene.Triangles.ToArray();
        if (triangles.Length == 0)
        {
            triangles = new[]
            {
                new TriangleEntity(
                    new Core.Math.Vector3(0, 0, 0),
                    new Core.Math.Vector3(1, 0, 0),
                    new Core.Math.Vector3(0, 1, 0),
                    new Color(1, 0, 0)
                )
            };
        }

        ulong bufferSize = (ulong)(sizeof(TriangleData) * triangles.Length);
        CreateBuffer(bufferSize, BufferUsageFlags.StorageBufferBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out _triangleBuffer, out _triangleBufferMemory);

        void* data;
        _vk.MapMemory(_device, _triangleBufferMemory, 0, bufferSize, 0, &data);

        var triangleData = new TriangleData[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            triangleData[i] = new TriangleData
            {
                V0 = new Vector3(triangles[i].V0.X, triangles[i].V0.Y, triangles[i].V0.Z),
                V1 = new Vector3(triangles[i].V1.X, triangles[i].V1.Y, triangles[i].V1.Z),
                V2 = new Vector3(triangles[i].V2.X, triangles[i].V2.Y, triangles[i].V2.Z),
                Color = new Vector3(triangles[i].Color.R, triangles[i].Color.G, triangles[i].Color.B)
            };
        }

        fixed (TriangleData* trianglePtr = triangleData)
        {
            System.Buffer.MemoryCopy(trianglePtr, data, bufferSize, bufferSize);
        }

        _vk.UnmapMemory(_device, _triangleBufferMemory);
    }

    private void CreateBuffer(ulong size, BufferUsageFlags usage, MemoryPropertyFlags properties,
        out Buffer buffer, out DeviceMemory bufferMemory)
    {
        var bufferInfo = new BufferCreateInfo
        {
            SType = StructureType.BufferCreateInfo,
            Size = size,
            Usage = usage,
            SharingMode = SharingMode.Exclusive
        };

        if (_vk.CreateBuffer(_device, &bufferInfo, null, out buffer) != Result.Success)
        {
            throw new Exception("Failed to create buffer");
        }

        MemoryRequirements memRequirements;
        _vk.GetBufferMemoryRequirements(_device, buffer, &memRequirements);

        var allocInfo = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = FindMemoryType(memRequirements.MemoryTypeBits, properties)
        };

        if (_vk.AllocateMemory(_device, &allocInfo, null, out bufferMemory) != Result.Success)
        {
            throw new Exception("Failed to allocate buffer memory");
        }

        _vk.BindBufferMemory(_device, buffer, bufferMemory, 0);
    }

    private void CreateDescriptorPool()
    {
        var poolSizes = stackalloc DescriptorPoolSize[3];
        poolSizes[0] = new DescriptorPoolSize
        {
            Type = DescriptorType.StorageImage,
            DescriptorCount = 1
        };
        poolSizes[1] = new DescriptorPoolSize
        {
            Type = DescriptorType.UniformBuffer,
            DescriptorCount = 2
        };
        poolSizes[2] = new DescriptorPoolSize
        {
            Type = DescriptorType.StorageBuffer,
            DescriptorCount = 2
        };

        var poolInfo = new DescriptorPoolCreateInfo
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            PoolSizeCount = 3,
            PPoolSizes = poolSizes,
            MaxSets = 1
        };

        if (_vk.CreateDescriptorPool(_device, &poolInfo, null, out _descriptorPool) != Result.Success)
        {
            throw new Exception("Failed to create descriptor pool");
        }
    }

    private void CreateDescriptorSet()
    {
        fixed (DescriptorSetLayout* layoutPtr = &_descriptorSetLayout)
        {
            var allocInfo = new DescriptorSetAllocateInfo
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = _descriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = layoutPtr
            };

            if (_vk.AllocateDescriptorSets(_device, &allocInfo, out _descriptorSet) != Result.Success)
            {
                throw new Exception("Failed to allocate descriptor set");
            }
        }
    }

    private void UpdateDescriptorSets()
    {
        var imageInfo = new DescriptorImageInfo
        {
            ImageLayout = ImageLayout.General,
            ImageView = _storageImageView
        };

        var cameraBufferInfo = new DescriptorBufferInfo
        {
            Buffer = _cameraBuffer,
            Offset = 0,
            Range = (ulong)sizeof(CameraUniformData)
        };

        var triangleBufferInfo = new DescriptorBufferInfo
        {
            Buffer = _triangleBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        var lightBufferInfo = new DescriptorBufferInfo
        {
            Buffer = _lightBuffer,
            Offset = 0,
            Range = (ulong)sizeof(LightUniformData)
        };

        var settingsBufferInfo = new DescriptorBufferInfo
        {
            Buffer = _settingsBuffer,
            Offset = 0,
            Range = (ulong)sizeof(RenderSettingsData)
        };

        var descriptorWrites = stackalloc WriteDescriptorSet[5];

        descriptorWrites[0] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = _descriptorSet,
            DstBinding = 0,
            DstArrayElement = 0,
            DescriptorType = DescriptorType.StorageImage,
            DescriptorCount = 1,
            PImageInfo = &imageInfo
        };

        descriptorWrites[1] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = _descriptorSet,
            DstBinding = 1,
            DstArrayElement = 0,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = &cameraBufferInfo
        };

        descriptorWrites[2] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = _descriptorSet,
            DstBinding = 2,
            DstArrayElement = 0,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = &triangleBufferInfo
        };

        descriptorWrites[3] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = _descriptorSet,
            DstBinding = 3,
            DstArrayElement = 0,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = &lightBufferInfo
        };

        descriptorWrites[4] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = _descriptorSet,
            DstBinding = 4,
            DstArrayElement = 0,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = &settingsBufferInfo
        };

        _vk.UpdateDescriptorSets(_device, 5, descriptorWrites, 0, null);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct CameraUniformData
{
    public Vector3 Position;
    public float Pad1;
    public Vector3 Target;
    public float Pad2;
    public Vector2 Resolution;
    public float Time;
    public float Fov;
}

[StructLayout(LayoutKind.Sequential)]
public struct TriangleData
{
    public Vector3 V0;
    public float Pad0;
    public Vector3 V1;
    public float Pad1;
    public Vector3 V2;
    public float Pad2;
    public Vector3 Color;
    public float Pad3;
}

[StructLayout(LayoutKind.Sequential)]
public struct LightData
{
    public int Type;
    public float Intensity;
    public float Pad1;
    public float Pad2;

    public float PositionX;
    public float PositionY;
    public float PositionZ;
    public float Pad3;

    public float DirectionX;
    public float DirectionY;
    public float DirectionZ;
    public float Pad4;

    public float ColorR;
    public float ColorG;
    public float ColorB;
    public float Pad5;
}

[StructLayout(LayoutKind.Sequential)]
public struct LightUniformData
{
    public int NumLights;
    public int Pad1;
    public int Pad2;
    public int Pad3;
    public LightData Light0;
    public LightData Light1;
    public LightData Light2;
    public LightData Light3;
    public LightData Light4;
    public LightData Light5;
    public LightData Light6;
    public LightData Light7;

    public void SetLight(int index, LightData light)
    {
        switch (index)
        {
            case 0: Light0 = light; break;
            case 1: Light1 = light; break;
            case 2: Light2 = light; break;
            case 3: Light3 = light; break;
            case 4: Light4 = light; break;
            case 5: Light5 = light; break;
            case 6: Light6 = light; break;
            case 7: Light7 = light; break;
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct RenderSettingsData
{
    public int MaxBounces;
    public int EnableShadows;
    public int EnableReflections;
    public float ReflectionStrength;
    public int ShadowSamples;
    public float ShadowSoftness;
    public Vector2 Pad;
}