using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Infrastructure.Vulkan.Tasks;

public unsafe class VulkanDeviceTask(Vk vk, KhrSurface khrSurface, SurfaceKHR surface)
{
    public PhysicalDevice PhysicalDevice { get; private set; }
    public Device Device { get; private set; }
    public uint QueueFamilyIndex { get; private set; }
    public Queue ComputeQueue { get; private set; }
    public Queue PresentQueue { get; private set; }
    public KhrSwapchain KhrSwapchain { get; private set; } = null!;

    public void SelectPhysicalDevice()
    {
        uint deviceCount = 0;
        vk.EnumeratePhysicalDevices(vk.CurrentInstance!.Value, &deviceCount, null);

        if (deviceCount == 0) throw new Exception("No Vulkan-capable GPU found");

        PhysicalDevice* devices = stackalloc PhysicalDevice[(int)deviceCount];
        vk.EnumeratePhysicalDevices(vk.CurrentInstance!.Value, &deviceCount, devices);

        for (int i = 0; i < deviceCount; i++)
        {
            PhysicalDevice device = devices[i];
            if (IsDeviceSuitable(device))
            {
                PhysicalDevice = device;
                PhysicalDeviceProperties props;
                vk.GetPhysicalDeviceProperties(device, &props);
                string? deviceName = Marshal.PtrToStringAnsi((nint)props.DeviceName);
                Console.WriteLine($"Selected GPU: {deviceName}");
                return;
            }
        }

        throw new Exception("No suitable GPU found");
    }

    private bool IsDeviceSuitable(PhysicalDevice device)
    {
        uint queueFamilyCount = 0;
        vk.GetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, null);

        QueueFamilyProperties* queueFamilies = stackalloc QueueFamilyProperties[(int)queueFamilyCount];
        vk.GetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, queueFamilies);

        for (uint i = 0; i < queueFamilyCount; i++)
        {
            bool hasCompute = (queueFamilies[i].QueueFlags & QueueFlags.ComputeBit) != 0;

            uint presentSupport = 0;
            khrSurface.GetPhysicalDeviceSurfaceSupport(device, i, surface, (Bool32*)&presentSupport);

            if (hasCompute && presentSupport != 0)
            {
                QueueFamilyIndex = i;
                return true;
            }
        }

        return false;
    }

    public void CreateLogicalDevice(Instance instance)
    {
        float queuePriority = 1.0f;
        DeviceQueueCreateInfo queueCreateInfo = new()
        {
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = QueueFamilyIndex,
            QueueCount = 1,
            PQueuePriorities = &queuePriority
        };

        PhysicalDeviceFeatures deviceFeatures = new();

        string[] extensions = new[] { "VK_KHR_swapchain" };
        IntPtr extensionNames = SilkMarshal.StringArrayToPtr(extensions);

        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = 1,
            PQueueCreateInfos = &queueCreateInfo,
            PEnabledFeatures = &deviceFeatures,
            EnabledExtensionCount = (uint)extensions.Length,
            PpEnabledExtensionNames = (byte**)extensionNames
        };

        if (vk.CreateDevice(PhysicalDevice, &createInfo, null, out Device device) != Result.Success)
            throw new Exception("Failed to create logical device");

        Device = device;

        vk.GetDeviceQueue(Device, QueueFamilyIndex, 0, out Queue computeQueue);
        ComputeQueue = computeQueue;
        PresentQueue = computeQueue;

        SilkMarshal.Free(extensionNames);

        if (!vk.TryGetDeviceExtension(instance, Device, out KhrSwapchain khrSwapchain))
            throw new Exception("KHR_swapchain extension not available");

        KhrSwapchain = khrSwapchain;
    }

    public uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties)
    {
        PhysicalDeviceMemoryProperties memProperties;
        vk.GetPhysicalDeviceMemoryProperties(PhysicalDevice, &memProperties);

        for (uint i = 0; i < memProperties.MemoryTypeCount; i++)
            if ((typeFilter & (1 << (int)i)) != 0 &&
                (memProperties.MemoryTypes[(int)i].PropertyFlags & properties) == properties)
                return i;

        throw new Exception("Failed to find suitable memory type");
    }

    public void Dispose()
    {
        if (Device.Handle != 0) vk.DestroyDevice(Device, null);
    }
}