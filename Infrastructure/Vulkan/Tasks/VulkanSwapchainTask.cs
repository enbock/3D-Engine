using Application;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Infrastructure.Vulkan.Tasks;

public unsafe class VulkanSwapchainTask(
    Vk vk,
    KhrSurface khrSurface,
    KhrSwapchain khrSwapchain,
    PhysicalDevice physicalDevice,
    Device device,
    SurfaceKHR surface,
    EngineConfig config)
{
    public SwapchainKHR Swapchain { get; private set; }
    private Format SwapchainFormat { get; set; }
    public Extent2D SwapchainExtent { get; private set; }
    public Image[] SwapchainImages { get; private set; } = Array.Empty<Image>();
    private ImageView[] SwapchainImageViews { get; set; } = Array.Empty<ImageView>();

    public void CreateSwapchain()
    {
        SurfaceCapabilitiesKHR capabilities;
        khrSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, surface, &capabilities);

        uint formatCount;
        khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, &formatCount, null);
        SurfaceFormatKHR* formats = stackalloc SurfaceFormatKHR[(int)formatCount];
        khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, &formatCount, formats);

        SwapchainFormat = formats[0].Format;
        ColorSpaceKHR colorSpace = formats[0].ColorSpace;

        for (int i = 0; i < formatCount; i++)
            if (formats[i].Format == Format.B8G8R8A8Srgb &&
                formats[i].ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
            {
                SwapchainFormat = formats[i].Format;
                colorSpace = formats[i].ColorSpace;
                break;
            }

        SwapchainExtent = new Extent2D
        {
            Width = (uint)config.Width,
            Height = (uint)config.Height
        };

        uint imageCount = capabilities.MinImageCount + 1;
        if (capabilities.MaxImageCount > 0 && imageCount > capabilities.MaxImageCount)
            imageCount = capabilities.MaxImageCount;

        SwapchainCreateInfoKHR createInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = surface,
            MinImageCount = imageCount,
            ImageFormat = SwapchainFormat,
            ImageColorSpace = colorSpace,
            ImageExtent = SwapchainExtent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.TransferDstBit | ImageUsageFlags.ColorAttachmentBit,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform = capabilities.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = config.VSync ? PresentModeKHR.FifoKhr : PresentModeKHR.ImmediateKhr,
            Clipped = true
        };

        if (khrSwapchain.CreateSwapchain(device, &createInfo, null, out SwapchainKHR swapchain) != Result.Success)
            throw new Exception("Failed to create swapchain");

        Swapchain = swapchain;

        uint swapchainImageCount;
        khrSwapchain.GetSwapchainImages(device, Swapchain, &swapchainImageCount, null);
        SwapchainImages = new Image[swapchainImageCount];
        fixed (Image* imagesPtr = SwapchainImages)
        {
            khrSwapchain.GetSwapchainImages(device, Swapchain, &swapchainImageCount, imagesPtr);
        }

        SwapchainImageViews = new ImageView[swapchainImageCount];
        for (int i = 0; i < swapchainImageCount; i++)
        {
            ImageViewCreateInfo viewInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = SwapchainImages[i],
                ViewType = ImageViewType.Type2D,
                Format = SwapchainFormat,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                }
            };

            if (vk.CreateImageView(device, &viewInfo, null, out SwapchainImageViews[i]) != Result.Success)
                throw new Exception("Failed to create image view");
        }
    }

    public void Cleanup()
    {
        foreach (ImageView imageView in SwapchainImageViews) vk.DestroyImageView(device, imageView, null);

        khrSwapchain.DestroySwapchain(device, Swapchain, null);
    }

    public void Dispose()
    {
        Cleanup();
    }
}