using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Infrastructure.Rendering.Vulkan.Tasks;

public unsafe class VulkanMultiPassTask(
    Vk vk,
    Device device,
    VulkanImageTask imageTask,
    VulkanPipelineTask pipelineTask)
{
    private Image denoisedShadowImage0;
    private Image denoisedShadowImage1;
    private DeviceMemory denoisedShadowMemory0;
    private DeviceMemory denoisedShadowMemory1;
    private ImageView denoisedShadowView0;
    private ImageView denoisedShadowView1;
    private DescriptorPool descriptorPool;

    // New: downsampled indirect image (small) and upscale pipeline resources
    private Image downsampledIndirectImage;
    private DeviceMemory downsampledIndirectMemory;
    private ImageView downsampledIndirectView;

    private Image downsampledShadowImage0;

    private Image downsampledShadowImage1;
    private DeviceMemory downsampledShadowMemory0;
    private DeviceMemory downsampledShadowMemory1;
    private ImageView downsampledShadowView0;
    private ImageView downsampledShadowView1;

    private Image gAlbedoImage;
    private DeviceMemory gAlbedoMemory;
    private ImageView gAlbedoView;
    private uint giDownsampleHeight;

    private uint giDownsampleWidth;

    private Image gNormalImage;
    private DeviceMemory gNormalMemory;
    private ImageView gNormalView;

    private Image gPositionImage;
    private DeviceMemory gPositionMemory;
    private ImageView gPositionView;

    private Image gRayDirImage;
    private DeviceMemory gRayDirMemory;
    private ImageView gRayDirView;
    private Image indirectColorImage;
    private DeviceMemory indirectColorMemory;
    private ImageView indirectColorView;

    private Image litColorImage;
    private DeviceMemory litColorMemory;
    private ImageView litColorView;

    private DescriptorSetLayout pass1DescriptorLayout;

    private DescriptorSet pass1DescriptorSet;

    private PipelineLayout pass1Layout;

    private Pipeline pass1Pipeline;

    private ShaderModule pass1Shader;
    private DescriptorSetLayout pass2BDescriptorLayout;
    private DescriptorSet pass2BDescriptorSet;
    private PipelineLayout pass2BLayout;
    private Pipeline pass2BPipeline;
    private ShaderModule pass2BShader;

    private DescriptorSetLayout pass2BShadowDenoiseDescriptorLayout;
    private DescriptorSet pass2BShadowDenoiseDescriptorSet;
    private PipelineLayout pass2BShadowDenoiseLayout;
    private Pipeline pass2BShadowDenoisePipeline;
    private ShaderModule pass2BShadowDenoiseShader;

    private DescriptorSetLayout pass2BShadowDescriptorLayout;
    private DescriptorSet pass2BShadowDescriptorSet;
    private PipelineLayout pass2BShadowLayout;
    private Pipeline pass2BShadowPipeline;
    private ShaderModule pass2BShadowShader;

    private DescriptorSetLayout pass2BShadowUpDescriptorLayout;
    private DescriptorSet pass2BShadowUpDescriptorSet;
    private PipelineLayout pass2BShadowUpLayout;
    private Pipeline pass2BShadowUpPipeline;
    private ShaderModule pass2BShadowUpShader;

    private DescriptorSetLayout pass2BUpDescriptorLayout;
    private DescriptorSet pass2BUpDescriptorSet;
    private PipelineLayout pass2BUpLayout;
    private Pipeline pass2BUpPipeline;
    private ShaderModule pass2BUpShader;
    private DescriptorSetLayout pass2DescriptorLayout;
    private DescriptorSet pass2DescriptorSet;
    private PipelineLayout pass2Layout;
    private Pipeline pass2Pipeline;
    private ShaderModule pass2Shader;
    private DescriptorSetLayout pass3DescriptorLayout;
    private DescriptorSet pass3DescriptorSet;
    private PipelineLayout pass3Layout;
    private Pipeline pass3Pipeline;
    private ShaderModule pass3Shader;
    private DescriptorSetLayout pass4DescriptorLayout;
    private DescriptorSet pass4DescriptorSet;
    private PipelineLayout pass4Layout;
    private Pipeline pass4Pipeline;
    private ShaderModule pass4Shader;

    private Image reflectedColorImage;
    private DeviceMemory reflectedColorMemory;
    private ImageView reflectedColorView;
    private uint shadowDownsampleHeight;

    private uint shadowDownsampleWidth;

    private Image shadowFullResImage0;

    private Image shadowFullResImage1;
    private DeviceMemory shadowFullResMemory0;
    private DeviceMemory shadowFullResMemory1;
    private ImageView shadowFullResView0;
    private ImageView shadowFullResView1;

    public void CreateGBufferImages(uint width, uint height, int giScale, int shadowScale)
    {
        Format format = Format.R32G32B32A32Sfloat;

        imageTask.CreateImage(width, height, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out gPositionImage, out gPositionMemory);
        gPositionView = imageTask.CreateImageView(gPositionImage, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(gPositionImage, ImageLayout.Undefined, ImageLayout.General);

        imageTask.CreateImage(width, height, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out gNormalImage, out gNormalMemory);
        gNormalView = imageTask.CreateImageView(gNormalImage, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(gNormalImage, ImageLayout.Undefined, ImageLayout.General);

        imageTask.CreateImage(width, height, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out gAlbedoImage, out gAlbedoMemory);
        gAlbedoView = imageTask.CreateImageView(gAlbedoImage, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(gAlbedoImage, ImageLayout.Undefined, ImageLayout.General);

        imageTask.CreateImage(width, height, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out gRayDirImage, out gRayDirMemory);
        gRayDirView = imageTask.CreateImageView(gRayDirImage, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(gRayDirImage, ImageLayout.Undefined, ImageLayout.General);

        imageTask.CreateImage(width, height, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out litColorImage, out litColorMemory);
        litColorView = imageTask.CreateImageView(litColorImage, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(litColorImage, ImageLayout.Undefined, ImageLayout.General);

        imageTask.CreateImage(width, height, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out indirectColorImage, out indirectColorMemory);
        indirectColorView = imageTask.CreateImageView(indirectColorImage, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(indirectColorImage, ImageLayout.Undefined, ImageLayout.General);

        // Create downsampled GI image according to giScale
        int gisc = Math.Max(1, giScale);
        giDownsampleWidth = (width + (uint)gisc - 1) / (uint)gisc;
        giDownsampleHeight = (height + (uint)gisc - 1) / (uint)gisc;

        imageTask.CreateImage(giDownsampleWidth, giDownsampleHeight, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out downsampledIndirectImage, out downsampledIndirectMemory);
        downsampledIndirectView = imageTask.CreateImageView(downsampledIndirectImage, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(downsampledIndirectImage, ImageLayout.Undefined, ImageLayout.General);

        // Create downsampled shadow images (two RGBA to support up to 8 lights)
        int ssc = Math.Max(1, shadowScale);
        shadowDownsampleWidth = (width + (uint)ssc - 1) / (uint)ssc;
        shadowDownsampleHeight = (height + (uint)ssc - 1) / (uint)ssc;

        imageTask.CreateImage(shadowDownsampleWidth, shadowDownsampleHeight, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out downsampledShadowImage0, out downsampledShadowMemory0);
        downsampledShadowView0 = imageTask.CreateImageView(downsampledShadowImage0, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(downsampledShadowImage0, ImageLayout.Undefined, ImageLayout.General);

        imageTask.CreateImage(shadowDownsampleWidth, shadowDownsampleHeight, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out downsampledShadowImage1, out downsampledShadowMemory1);
        downsampledShadowView1 = imageTask.CreateImageView(downsampledShadowImage1, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(downsampledShadowImage1, ImageLayout.Undefined, ImageLayout.General);

        imageTask.CreateImage(shadowDownsampleWidth, shadowDownsampleHeight, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out denoisedShadowImage0, out denoisedShadowMemory0);
        denoisedShadowView0 = imageTask.CreateImageView(denoisedShadowImage0, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(denoisedShadowImage0, ImageLayout.Undefined, ImageLayout.General);

        imageTask.CreateImage(shadowDownsampleWidth, shadowDownsampleHeight, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out denoisedShadowImage1, out denoisedShadowMemory1);
        denoisedShadowView1 = imageTask.CreateImageView(denoisedShadowImage1, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(denoisedShadowImage1, ImageLayout.Undefined, ImageLayout.General);

        imageTask.CreateImage(width, height, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out shadowFullResImage0, out shadowFullResMemory0);
        shadowFullResView0 = imageTask.CreateImageView(shadowFullResImage0, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(shadowFullResImage0, ImageLayout.Undefined, ImageLayout.General);

        imageTask.CreateImage(width, height, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out shadowFullResImage1, out shadowFullResMemory1);
        shadowFullResView1 = imageTask.CreateImageView(shadowFullResImage1, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(shadowFullResImage1, ImageLayout.Undefined, ImageLayout.General);

        imageTask.CreateImage(width, height, format,
            ImageTiling.Optimal,
            ImageUsageFlags.StorageBit | ImageUsageFlags.SampledBit,
            MemoryPropertyFlags.DeviceLocalBit,
            out reflectedColorImage, out reflectedColorMemory);
        reflectedColorView = imageTask.CreateImageView(reflectedColorImage, format, ImageAspectFlags.ColorBit);
        imageTask.TransitionImageLayout(reflectedColorImage, ImageLayout.Undefined, ImageLayout.General);
    }

    public void CreatePipelines()
    {
        CreatePass1Pipeline();
        CreatePass2BShadowPipeline();
        CreatePass2BShadowDenoisePipeline();
        CreatePass2BShadowUpscalePipeline();
        CreatePass2Pipeline();
        CreatePass2BPipeline();
        CreatePass2BUpscalePipeline();
        CreatePass3Pipeline();
        CreatePass4Pipeline();
    }

    private void CreatePass1Pipeline()
    {
        DescriptorSetLayoutBinding[] bindings =
        [
            new()
            {
                Binding = 0,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 1,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 2,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 3,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 4,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 5,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 6,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }
        ];

        pass1DescriptorLayout = pipelineTask.CreateDescriptorSetLayout(bindings);

        byte[] shaderCode = LoadShaderCode("shader/pass1_primary.comp.spv");
        pass1Shader = pipelineTask.CreateShaderModule(shaderCode);

        pass1Layout = pipelineTask.CreatePipelineLayout(pass1DescriptorLayout);
        pass1Pipeline = pipelineTask.CreateComputePipeline(pass1Shader, pass1Layout);
    }

    private void CreatePass2Pipeline()
    {
        DescriptorSetLayoutBinding[] bindings =
        [
            new()
            {
                Binding = 0,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 1,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 2,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 3,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 4,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 5,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 6,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 7,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 8,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 9,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 10,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 11,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }
        ];

        pass2DescriptorLayout = pipelineTask.CreateDescriptorSetLayout(bindings);

        byte[] shaderCode = LoadShaderCode("shader/pass2_lighting.comp.spv");
        pass2Shader = pipelineTask.CreateShaderModule(shaderCode);

        pass2Layout = pipelineTask.CreatePipelineLayout(pass2DescriptorLayout);
        pass2Pipeline = pipelineTask.CreateComputePipeline(pass2Shader, pass2Layout);
    }

    private void CreatePass2BPipeline()
    {
        DescriptorSetLayoutBinding[] bindings =
        [
            new()
            {
                Binding = 0,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 1,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 2,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 3,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 4,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 5,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 6,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 7,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 8,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 9,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }
        ];

        pass2BDescriptorLayout = pipelineTask.CreateDescriptorSetLayout(bindings);

        byte[] shaderCode = LoadShaderCode("shader/pass2b_indirect_downsample.comp.spv");
        pass2BShader = pipelineTask.CreateShaderModule(shaderCode);

        pass2BLayout = pipelineTask.CreatePipelineLayout(pass2BDescriptorLayout);
        pass2BPipeline = pipelineTask.CreateComputePipeline(pass2BShader, pass2BLayout);
    }

    private void CreatePass2BUpscalePipeline()
    {
        DescriptorSetLayoutBinding[] bindings =
        [
            new()
            {
                Binding = 0,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 1,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 2,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 3,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 4,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }
        ];

        pass2BUpDescriptorLayout = pipelineTask.CreateDescriptorSetLayout(bindings);

        byte[] shaderCode = LoadShaderCode("shader/pass2b_indirect_upscale.comp.spv");
        pass2BUpShader = pipelineTask.CreateShaderModule(shaderCode);

        pass2BUpLayout = pipelineTask.CreatePipelineLayout(pass2BUpDescriptorLayout);
        pass2BUpPipeline = pipelineTask.CreateComputePipeline(pass2BUpShader, pass2BUpLayout);
    }

    private void CreatePass2BShadowPipeline()
    {
        DescriptorSetLayoutBinding[] bindings =
        [
            new()
            {
                Binding = 0,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }, // gPosition
            new()
            {
                Binding = 1,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }, // gNormal
            new()
            {
                Binding = 2,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }, // gAlbedo
            new()
            {
                Binding = 3,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }, // litColor
            new()
            {
                Binding = 4,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }, // downsampledShadow0 (write)
            new()
            {
                Binding = 5,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }, // downsampledShadow1 (write)
            new()
            {
                Binding = 6,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 7,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 8,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 9,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 10,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }
        ];

        pass2BShadowDescriptorLayout = pipelineTask.CreateDescriptorSetLayout(bindings);

        byte[] shaderCode = LoadShaderCode("shader/pass2b_shadow_downsample.comp.spv");
        pass2BShadowShader = pipelineTask.CreateShaderModule(shaderCode);

        pass2BShadowLayout = pipelineTask.CreatePipelineLayout(pass2BShadowDescriptorLayout);
        pass2BShadowPipeline = pipelineTask.CreateComputePipeline(pass2BShadowShader, pass2BShadowLayout);
    }

    private void CreatePass2BShadowDenoisePipeline()
    {
        DescriptorSetLayoutBinding[] bindings =
        [
            new()
            {
                Binding = 0,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 1,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 2,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 3,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 4,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 5,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 6,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }
        ];

        pass2BShadowDenoiseDescriptorLayout = pipelineTask.CreateDescriptorSetLayout(bindings);

        byte[] shaderCode = LoadShaderCode("shader/pass2b_shadow_denoise.comp.spv");
        pass2BShadowDenoiseShader = pipelineTask.CreateShaderModule(shaderCode);

        pass2BShadowDenoiseLayout = pipelineTask.CreatePipelineLayout(pass2BShadowDenoiseDescriptorLayout);
        pass2BShadowDenoisePipeline = pipelineTask.CreateComputePipeline(pass2BShadowDenoiseShader, pass2BShadowDenoiseLayout);
    }

    private void CreatePass2BShadowUpscalePipeline()
    {
        DescriptorSetLayoutBinding[] bindings =
        [
            new()
            {
                Binding = 0,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 1,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 2,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 3,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 4,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }
        ];

        pass2BShadowUpDescriptorLayout = pipelineTask.CreateDescriptorSetLayout(bindings);

        byte[] shaderCode = LoadShaderCode("shader/pass2b_shadow_upscale.comp.spv");
        pass2BShadowUpShader = pipelineTask.CreateShaderModule(shaderCode);

        pass2BShadowUpLayout = pipelineTask.CreatePipelineLayout(pass2BShadowUpDescriptorLayout);
        pass2BShadowUpPipeline = pipelineTask.CreateComputePipeline(pass2BShadowUpShader, pass2BShadowUpLayout);
    }

    private void CreatePass3Pipeline()
    {
        DescriptorSetLayoutBinding[] bindings =
        [
            new()
            {
                Binding = 0,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 1,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 2,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 3,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 4,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 5,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 6,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 7,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 8,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 9,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 10,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }
        ];

        pass3DescriptorLayout = pipelineTask.CreateDescriptorSetLayout(bindings);

        byte[] shaderCode = LoadShaderCode("shader/pass3_reflections.comp.spv");
        pass3Shader = pipelineTask.CreateShaderModule(shaderCode);

        pass3Layout = pipelineTask.CreatePipelineLayout(pass3DescriptorLayout);
        pass3Pipeline = pipelineTask.CreateComputePipeline(pass3Shader, pass3Layout);
    }

    private void CreatePass4Pipeline()
    {
        DescriptorSetLayoutBinding[] bindings =
        [
            new()
            {
                Binding = 0,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 1,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 2,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 3,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            }
        ];

        pass4DescriptorLayout = pipelineTask.CreateDescriptorSetLayout(bindings);

        byte[] shaderCode = LoadShaderCode("shader/pass4_composite.comp.spv");
        pass4Shader = pipelineTask.CreateShaderModule(shaderCode);

        pass4Layout = pipelineTask.CreatePipelineLayout(pass4DescriptorLayout);
        pass4Pipeline = pipelineTask.CreateComputePipeline(pass4Shader, pass4Layout);
    }

    public void CreateDescriptorSets(
        Buffer cameraBuffer,
        Buffer triangleBuffer,
        Buffer lightBuffer,
        Buffer settingsBuffer,
        Buffer bvhBuffer,
        ImageView storageImageView)
    {
        DescriptorPoolSize[] poolSizes =
        [
            new()
            {
                Type = DescriptorType.StorageImage,
                DescriptorCount = 60
            },
            new()
            {
                Type = DescriptorType.StorageBuffer,
                DescriptorCount = 40
            },
            new()
            {
                Type = DescriptorType.UniformBuffer,
                DescriptorCount = 30
            }
        ];

        fixed (DescriptorPoolSize* pPoolSizes = poolSizes)
        {
            DescriptorPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = (uint)poolSizes.Length,
                PPoolSizes = pPoolSizes,
                MaxSets = 13
            };

            if (vk.CreateDescriptorPool(device, &poolInfo, null, out descriptorPool) != Result.Success)
                throw new Exception("Failed to create descriptor pool");
        }

        AllocateDescriptorSet(pass1DescriptorLayout, out pass1DescriptorSet);
        AllocateDescriptorSet(pass2DescriptorLayout, out pass2DescriptorSet);
        AllocateDescriptorSet(pass2BShadowDescriptorLayout, out pass2BShadowDescriptorSet);
        AllocateDescriptorSet(pass2BShadowDenoiseDescriptorLayout, out pass2BShadowDenoiseDescriptorSet);
        AllocateDescriptorSet(pass2BDescriptorLayout, out pass2BDescriptorSet);
        AllocateDescriptorSet(pass2BUpDescriptorLayout, out pass2BUpDescriptorSet);
        AllocateDescriptorSet(pass2BShadowUpDescriptorLayout, out pass2BShadowUpDescriptorSet);
        AllocateDescriptorSet(pass3DescriptorLayout, out pass3DescriptorSet);
        AllocateDescriptorSet(pass4DescriptorLayout, out pass4DescriptorSet);

        UpdatePass1DescriptorSet(cameraBuffer, triangleBuffer, bvhBuffer);
        UpdatePass2DescriptorSet(cameraBuffer, triangleBuffer, lightBuffer, settingsBuffer, bvhBuffer);
        UpdatePass2BShadowDescriptorSet(cameraBuffer, triangleBuffer, lightBuffer, settingsBuffer, bvhBuffer);
        UpdatePass2BShadowDenoiseDescriptorSet(settingsBuffer);
        UpdatePass2BDescriptorSet(cameraBuffer, triangleBuffer, lightBuffer, settingsBuffer, bvhBuffer);
        UpdatePass2BUpDescriptorSet(cameraBuffer, settingsBuffer);
        UpdatePass2BShadowUpDescriptorSet(cameraBuffer, settingsBuffer);
        UpdatePass3DescriptorSet(cameraBuffer, triangleBuffer, lightBuffer, settingsBuffer, bvhBuffer);
        UpdatePass4DescriptorSet(cameraBuffer, settingsBuffer, storageImageView);
    }

    private void AllocateDescriptorSet(DescriptorSetLayout layout, out DescriptorSet set)
    {
        DescriptorSetAllocateInfo allocInfo = new()
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = descriptorPool,
            DescriptorSetCount = 1,
            PSetLayouts = &layout
        };

        fixed (DescriptorSet* pSet = &set)
        {
            if (vk.AllocateDescriptorSets(device, &allocInfo, pSet) != Result.Success)
                throw new Exception("Failed to allocate descriptor set");
        }
    }

    private void UpdatePass1DescriptorSet(Buffer cameraBuffer, Buffer triangleBuffer, Buffer bvhBuffer)
    {
        DescriptorImageInfo[] imageInfos =
        [
            new()
            {
                ImageView = gPositionView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gNormalView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gAlbedoView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gRayDirView,
                ImageLayout = ImageLayout.General
            }
        ];

        DescriptorBufferInfo cameraBufferInfo = new()
        {
            Buffer = cameraBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        DescriptorBufferInfo triangleBufferInfo = new()
        {
            Buffer = triangleBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        DescriptorBufferInfo bvhBufferInfo = new()
        {
            Buffer = bvhBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        WriteDescriptorSet[] writes = new WriteDescriptorSet[7];

        DescriptorBufferInfo* pCameraInfo = &cameraBufferInfo;
        DescriptorBufferInfo* pTriangleInfo = &triangleBufferInfo;
        DescriptorBufferInfo* pBvhInfo = &bvhBufferInfo;

        DescriptorImageInfo* pImageInfos = stackalloc DescriptorImageInfo[4];
        pImageInfos[0] = imageInfos[0];
        pImageInfos[1] = imageInfos[1];
        pImageInfos[2] = imageInfos[2];
        pImageInfos[3] = imageInfos[3];

        for (uint i = 0; i < 4; i++)
            writes[i] = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = pass1DescriptorSet,
                DstBinding = i,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                PImageInfo = &pImageInfos[i]
            };

        writes[4] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass1DescriptorSet,
            DstBinding = 4,
            DstArrayElement = 0,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pCameraInfo
        };

        writes[5] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass1DescriptorSet,
            DstBinding = 5,
            DstArrayElement = 0,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pTriangleInfo
        };

        writes[6] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass1DescriptorSet,
            DstBinding = 6,
            DstArrayElement = 0,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pBvhInfo
        };

        fixed (WriteDescriptorSet* pWrites = writes)
        {
            vk.UpdateDescriptorSets(device, (uint)writes.Length, pWrites, 0, null);
        }
    }

    private void UpdatePass2DescriptorSet(
        Buffer cameraBuffer,
        Buffer triangleBuffer,
        Buffer lightBuffer,
        Buffer settingsBuffer,
        Buffer bvhBuffer)
    {
        DescriptorImageInfo[] imageInfos =
        [
            new()
            {
                ImageView = gPositionView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gNormalView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gAlbedoView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gRayDirView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = litColorView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = shadowFullResView0,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = shadowFullResView1,
                ImageLayout = ImageLayout.General
            }
        ];

        DescriptorBufferInfo lightBufferInfo = new()
        {
            Buffer = lightBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo triangleBufferInfo = new()
        {
            Buffer = triangleBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo settingsBufferInfo = new()
        {
            Buffer = settingsBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo cameraBufferInfo = new()
        {
            Buffer = cameraBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo bvhBufferInfo = new()
        {
            Buffer = bvhBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        WriteDescriptorSet[] writes = new WriteDescriptorSet[12];

        DescriptorImageInfo* pImageInfos = stackalloc DescriptorImageInfo[7];
        for (int i = 0; i < 7; i++) pImageInfos[i] = imageInfos[i];

        for (uint i = 0; i < 7; i++)
            writes[i] = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = pass2DescriptorSet,
                DstBinding = i,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                PImageInfo = &pImageInfos[i]
            };

        DescriptorBufferInfo* pLightInfo = &lightBufferInfo;
        DescriptorBufferInfo* pTriangleInfo = &triangleBufferInfo;
        DescriptorBufferInfo* pSettingsInfo = &settingsBufferInfo;
        DescriptorBufferInfo* pCameraInfo = &cameraBufferInfo;
        DescriptorBufferInfo* pBvhInfo = &bvhBufferInfo;

        writes[7] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2DescriptorSet,
            DstBinding = 7,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pLightInfo
        };
        writes[8] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2DescriptorSet,
            DstBinding = 8,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pTriangleInfo
        };
        writes[9] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2DescriptorSet,
            DstBinding = 9,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pSettingsInfo
        };
        writes[10] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2DescriptorSet,
            DstBinding = 10,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pCameraInfo
        };
        writes[11] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2DescriptorSet,
            DstBinding = 11,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pBvhInfo
        };

        fixed (WriteDescriptorSet* pWrites = writes)
        {
            vk.UpdateDescriptorSets(device, (uint)writes.Length, pWrites, 0, null);
        }
    }

    private void UpdatePass2BDescriptorSet(
        Buffer cameraBuffer,
        Buffer triangleBuffer,
        Buffer lightBuffer,
        Buffer settingsBuffer,
        Buffer bvhBuffer)
    {
        DescriptorImageInfo[] imageInfos =
        [
            new()
            {
                ImageView = gPositionView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gNormalView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gAlbedoView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = litColorView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = downsampledIndirectView,
                ImageLayout = ImageLayout.General
            }
        ];

        DescriptorBufferInfo triangleBufferInfo = new()
        {
            Buffer = triangleBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo lightBufferInfo = new()
        {
            Buffer = lightBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo settingsBufferInfo = new()
        {
            Buffer = settingsBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo cameraBufferInfo = new()
        {
            Buffer = cameraBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo bvhBufferInfo = new()
        {
            Buffer = bvhBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        WriteDescriptorSet[] writes = new WriteDescriptorSet[10];

        DescriptorBufferInfo* pTriangleInfo = &triangleBufferInfo;
        DescriptorBufferInfo* pLightInfo = &lightBufferInfo;
        DescriptorBufferInfo* pSettingsInfo = &settingsBufferInfo;
        DescriptorBufferInfo* pCameraInfo = &cameraBufferInfo;
        DescriptorBufferInfo* pBvhInfo = &bvhBufferInfo;

        DescriptorImageInfo* pImageInfos = stackalloc DescriptorImageInfo[5];
        pImageInfos[0] = imageInfos[0];
        pImageInfos[1] = imageInfos[1];
        pImageInfos[2] = imageInfos[2];
        pImageInfos[3] = imageInfos[3];
        pImageInfos[4] = imageInfos[4];

        for (uint i = 0; i < 5; i++)
            writes[i] = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = pass2BDescriptorSet,
                DstBinding = i,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                PImageInfo = &pImageInfos[i]
            };

        writes[5] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BDescriptorSet,
            DstBinding = 5,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pTriangleInfo
        };

        writes[6] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BDescriptorSet,
            DstBinding = 6,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pLightInfo
        };

        writes[7] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BDescriptorSet,
            DstBinding = 7,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pSettingsInfo
        };

        writes[8] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BDescriptorSet,
            DstBinding = 8,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pCameraInfo
        };

        writes[9] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BDescriptorSet,
            DstBinding = 9,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pBvhInfo
        };

        fixed (WriteDescriptorSet* pWrites = writes)
        {
            vk.UpdateDescriptorSets(device, (uint)writes.Length, pWrites, 0, null);
        }
    }

    private void UpdatePass2BShadowDescriptorSet(Buffer cameraBuffer, Buffer triangleBuffer, Buffer lightBuffer, Buffer settingsBuffer, Buffer bvhBuffer)
    {
        DescriptorImageInfo[] imageInfos =
        [
            new()
            {
                ImageView = gPositionView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gNormalView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gAlbedoView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = litColorView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = downsampledShadowView0,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = downsampledShadowView1,
                ImageLayout = ImageLayout.General
            }
        ];

        DescriptorBufferInfo triangleBufferInfo = new()
        {
            Buffer = triangleBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo lightBufferInfo = new()
        {
            Buffer = lightBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo settingsBufferInfo = new()
        {
            Buffer = settingsBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo cameraBufferInfo = new()
        {
            Buffer = cameraBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo bvhBufferInfo = new()
        {
            Buffer = bvhBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        DescriptorImageInfo* pImageInfos = stackalloc DescriptorImageInfo[6];
        for (int i = 0; i < 6; i++) pImageInfos[i] = imageInfos[i];

        DescriptorBufferInfo* pTriangleInfo = &triangleBufferInfo;
        DescriptorBufferInfo* pLightInfo = &lightBufferInfo;
        DescriptorBufferInfo* pSettingsInfo = &settingsBufferInfo;
        DescriptorBufferInfo* pCameraInfo = &cameraBufferInfo;
        DescriptorBufferInfo* pBvhInfo = &bvhBufferInfo;

        WriteDescriptorSet[] writes = new WriteDescriptorSet[11];

        for (uint i = 0; i < 6; i++)
            writes[i] = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = pass2BShadowDescriptorSet,
                DstBinding = i,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                PImageInfo = &pImageInfos[i]
            };

        writes[6] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BShadowDescriptorSet,
            DstBinding = 6,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pTriangleInfo
        };
        writes[7] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BShadowDescriptorSet,
            DstBinding = 7,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pLightInfo
        };
        writes[8] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BShadowDescriptorSet,
            DstBinding = 8,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pSettingsInfo
        };
        writes[9] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BShadowDescriptorSet,
            DstBinding = 9,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pCameraInfo
        };
        writes[10] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BShadowDescriptorSet,
            DstBinding = 10,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pBvhInfo
        };

        fixed (WriteDescriptorSet* pWrites = writes)
        {
            vk.UpdateDescriptorSets(device, (uint)writes.Length, pWrites, 0, null);
        }
    }

    private void UpdatePass2BShadowDenoiseDescriptorSet(Buffer settingsBuffer)
    {
        DescriptorImageInfo[] imageInfos =
        [
            new()
            {
                ImageView = downsampledShadowView0,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = denoisedShadowView0,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = downsampledShadowView1,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = denoisedShadowView1,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gPositionView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gNormalView,
                ImageLayout = ImageLayout.General
            }
        ];

        DescriptorBufferInfo settingsBufferInfo = new()
        {
            Buffer = settingsBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        DescriptorImageInfo* pImageInfos = stackalloc DescriptorImageInfo[6];
        for (int i = 0; i < 6; i++) pImageInfos[i] = imageInfos[i];

        DescriptorBufferInfo* pSettingsInfo = &settingsBufferInfo;

        WriteDescriptorSet[] writes = new WriteDescriptorSet[7];

        for (uint i = 0; i < 6; i++)
            writes[i] = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = pass2BShadowDenoiseDescriptorSet,
                DstBinding = i,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                PImageInfo = &pImageInfos[i]
            };

        writes[6] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BShadowDenoiseDescriptorSet,
            DstBinding = 6,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pSettingsInfo
        };

        fixed (WriteDescriptorSet* pWrites = writes)
        {
            vk.UpdateDescriptorSets(device, (uint)writes.Length, pWrites, 0, null);
        }
    }

    private void UpdatePass2BUpDescriptorSet(Buffer cameraBuffer, Buffer settingsBuffer)
    {
        DescriptorImageInfo ds = new()
        {
            ImageView = downsampledIndirectView,
            ImageLayout = ImageLayout.General
        };
        DescriptorImageInfo outImg = new()
        {
            ImageView = indirectColorView,
            ImageLayout = ImageLayout.General
        };
        DescriptorImageInfo litImg = new()
        {
            ImageView = litColorView,
            ImageLayout = ImageLayout.General
        };

        DescriptorBufferInfo cameraBufferInfo = new()
        {
            Buffer = cameraBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo settingsBufferInfo = new()
        {
            Buffer = settingsBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        DescriptorImageInfo* pDs = &ds;
        DescriptorImageInfo* pOut = &outImg;
        DescriptorImageInfo* pLit = &litImg;

        DescriptorBufferInfo* pCamera = &cameraBufferInfo;
        DescriptorBufferInfo* pSettings = &settingsBufferInfo;

        WriteDescriptorSet[] writes = new WriteDescriptorSet[5];
        writes[0] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BUpDescriptorSet,
            DstBinding = 0,
            DescriptorType = DescriptorType.StorageImage,
            DescriptorCount = 1,
            PImageInfo = pDs
        };
        writes[1] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BUpDescriptorSet,
            DstBinding = 1,
            DescriptorType = DescriptorType.StorageImage,
            DescriptorCount = 1,
            PImageInfo = pOut
        };
        writes[2] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BUpDescriptorSet,
            DstBinding = 2,
            DescriptorType = DescriptorType.StorageImage,
            DescriptorCount = 1,
            PImageInfo = pLit
        };
        writes[3] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BUpDescriptorSet,
            DstBinding = 3,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pCamera
        };
        writes[4] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BUpDescriptorSet,
            DstBinding = 4,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pSettings
        };

        fixed (WriteDescriptorSet* pWrites = writes)
        {
            vk.UpdateDescriptorSets(device, (uint)writes.Length, pWrites, 0, null);
        }
    }

    private void UpdatePass2BShadowUpDescriptorSet(Buffer cameraBuffer, Buffer settingsBuffer)
    {
        DescriptorImageInfo ds0 = new()
        {
            ImageView = denoisedShadowView0,
            ImageLayout = ImageLayout.General
        };
        DescriptorImageInfo out0 = new()
        {
            ImageView = shadowFullResView0,
            ImageLayout = ImageLayout.General
        };
        DescriptorImageInfo ds1 = new()
        {
            ImageView = denoisedShadowView1,
            ImageLayout = ImageLayout.General
        };
        DescriptorImageInfo out1 = new()
        {
            ImageView = shadowFullResView1,
            ImageLayout = ImageLayout.General
        };

        DescriptorBufferInfo settingsBufferInfo = new()
        {
            Buffer = settingsBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        DescriptorImageInfo* pDs0 = &ds0;
        DescriptorImageInfo* pOut0 = &out0;
        DescriptorImageInfo* pDs1 = &ds1;
        DescriptorImageInfo* pOut1 = &out1;

        DescriptorBufferInfo* pSettings = &settingsBufferInfo;

        WriteDescriptorSet[] writes = new WriteDescriptorSet[5];
        writes[0] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BShadowUpDescriptorSet,
            DstBinding = 0,
            DescriptorType = DescriptorType.StorageImage,
            DescriptorCount = 1,
            PImageInfo = pDs0
        };
        writes[1] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BShadowUpDescriptorSet,
            DstBinding = 1,
            DescriptorType = DescriptorType.StorageImage,
            DescriptorCount = 1,
            PImageInfo = pOut0
        };
        writes[2] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BShadowUpDescriptorSet,
            DstBinding = 2,
            DescriptorType = DescriptorType.StorageImage,
            DescriptorCount = 1,
            PImageInfo = pDs1
        };
        writes[3] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BShadowUpDescriptorSet,
            DstBinding = 3,
            DescriptorType = DescriptorType.StorageImage,
            DescriptorCount = 1,
            PImageInfo = pOut1
        };
        writes[4] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass2BShadowUpDescriptorSet,
            DstBinding = 4,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pSettings
        };

        fixed (WriteDescriptorSet* pWrites = writes)
        {
            vk.UpdateDescriptorSets(device, (uint)writes.Length, pWrites, 0, null);
        }
    }


    private void UpdatePass3DescriptorSet(
        Buffer cameraBuffer,
        Buffer triangleBuffer,
        Buffer lightBuffer,
        Buffer settingsBuffer,
        Buffer bvhBuffer)
    {
        DescriptorImageInfo[] imageInfos =
        [
            new()
            {
                ImageView = gPositionView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gNormalView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gAlbedoView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = gRayDirView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = indirectColorView,
                ImageLayout = ImageLayout.General
            },
            new()
            {
                ImageView = reflectedColorView,
                ImageLayout = ImageLayout.General
            }
        ];

        DescriptorBufferInfo triangleBufferInfo = new()
        {
            Buffer = triangleBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo lightBufferInfo = new()
        {
            Buffer = lightBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo settingsBufferInfo = new()
        {
            Buffer = settingsBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo cameraBufferInfo = new()
        {
            Buffer = cameraBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };
        DescriptorBufferInfo bvhBufferInfo = new()
        {
            Buffer = bvhBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        WriteDescriptorSet[] writes = new WriteDescriptorSet[11];

        DescriptorBufferInfo* pTriangleInfo = &triangleBufferInfo;
        DescriptorBufferInfo* pLightInfo = &lightBufferInfo;
        DescriptorBufferInfo* pSettingsInfo = &settingsBufferInfo;
        DescriptorBufferInfo* pCameraInfo = &cameraBufferInfo;
        DescriptorBufferInfo* pBvhInfo = &bvhBufferInfo;

        DescriptorImageInfo* pImageInfos = stackalloc DescriptorImageInfo[6];
        pImageInfos[0] = imageInfos[0];
        pImageInfos[1] = imageInfos[1];
        pImageInfos[2] = imageInfos[2];
        pImageInfos[3] = imageInfos[3];
        pImageInfos[4] = imageInfos[4];
        pImageInfos[5] = imageInfos[5];

        for (uint i = 0; i < 6; i++)
            writes[i] = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = pass3DescriptorSet,
                DstBinding = i,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                PImageInfo = &pImageInfos[i]
            };

        writes[6] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass3DescriptorSet,
            DstBinding = 6,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pTriangleInfo
        };

        writes[7] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass3DescriptorSet,
            DstBinding = 7,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pLightInfo
        };

        writes[8] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass3DescriptorSet,
            DstBinding = 8,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pSettingsInfo
        };

        writes[9] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass3DescriptorSet,
            DstBinding = 9,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pCameraInfo
        };

        writes[10] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass3DescriptorSet,
            DstBinding = 10,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = pBvhInfo
        };

        fixed (WriteDescriptorSet* pWrites = writes)
        {
            vk.UpdateDescriptorSets(device, (uint)writes.Length, pWrites, 0, null);
        }
    }

    private void UpdatePass4DescriptorSet(Buffer cameraBuffer, Buffer settingsBuffer, ImageView storageImageView)
    {
        DescriptorImageInfo reflectedImageInfo = new()
        {
            ImageView = reflectedColorView,
            ImageLayout = ImageLayout.General
        };

        DescriptorImageInfo outputImageInfo = new()
        {
            ImageView = storageImageView,
            ImageLayout = ImageLayout.General
        };

        DescriptorBufferInfo cameraBufferInfo = new()
        {
            Buffer = cameraBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        DescriptorBufferInfo settingsBufferInfo = new()
        {
            Buffer = settingsBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        DescriptorImageInfo* pReflectedInfo = &reflectedImageInfo;
        DescriptorImageInfo* pOutputInfo = &outputImageInfo;
        DescriptorBufferInfo* pCameraInfo = &cameraBufferInfo;
        DescriptorBufferInfo* pSettingsInfo = &settingsBufferInfo;

        WriteDescriptorSet[] writes = new WriteDescriptorSet[4];
        writes[0] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass4DescriptorSet,
            DstBinding = 0,
            DescriptorType = DescriptorType.StorageImage,
            DescriptorCount = 1,
            PImageInfo = pReflectedInfo
        };

        writes[1] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass4DescriptorSet,
            DstBinding = 1,
            DescriptorType = DescriptorType.StorageImage,
            DescriptorCount = 1,
            PImageInfo = pOutputInfo
        };

        writes[2] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass4DescriptorSet,
            DstBinding = 2,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pCameraInfo
        };

        writes[3] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pass4DescriptorSet,
            DstBinding = 3,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = pSettingsInfo
        };

        fixed (WriteDescriptorSet* pWrites = writes)
        {
            vk.UpdateDescriptorSets(device, (uint)writes.Length, pWrites, 0, null);
        }
    }

    public void RecordMultiPassCommands(
        CommandBuffer commandBuffer,
        Extent2D extent)
    {
        uint groupCountX = (extent.Width + 15) / 16;
        uint groupCountY = (extent.Height + 15) / 16;

        uint shadowGroupCountX = (shadowDownsampleWidth + 15) / 16;
        uint shadowGroupCountY = (shadowDownsampleHeight + 15) / 16;

        uint giGroupCountX = (giDownsampleWidth + 15) / 16;
        uint giGroupCountY = (giDownsampleHeight + 15) / 16;

        // dispatch pass1
        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pass1Pipeline);
        fixed (DescriptorSet* pDescSet1 = &pass1DescriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pass1Layout, 0, 1, pDescSet1, 0, null);
        }

        vk.CmdDispatch(commandBuffer, groupCountX, groupCountY, 1);

        InsertMemoryBarrier(commandBuffer);

        // Shadow downsample: dispatch at downsampled shadow size
        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pass2BShadowPipeline);
        fixed (DescriptorSet* pDescSet2BSh = &pass2BShadowDescriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pass2BShadowLayout, 0, 1, pDescSet2BSh, 0, null);
        }

        vk.CmdDispatch(commandBuffer, Math.Max(1, shadowGroupCountX), Math.Max(1, shadowGroupCountY), 1);

        InsertMemoryBarrier(commandBuffer);

        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pass2BShadowDenoisePipeline);
        fixed (DescriptorSet* pDescSet2BShDenoise = &pass2BShadowDenoiseDescriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pass2BShadowDenoiseLayout, 0, 1, pDescSet2BShDenoise, 0, null);
        }

        vk.CmdDispatch(commandBuffer, Math.Max(1, shadowGroupCountX), Math.Max(1, shadowGroupCountY), 1);

        InsertMemoryBarrier(commandBuffer);

        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pass2BShadowUpPipeline);
        fixed (DescriptorSet* pDescSet2BShUp = &pass2BShadowUpDescriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pass2BShadowUpLayout, 0, 1, pDescSet2BShUp, 0, null);
        }

        // upscale writes full-res, so use full-res group counts
        vk.CmdDispatch(commandBuffer, groupCountX, groupCountY, 1);

        InsertMemoryBarrier(commandBuffer);

        // pass2 (lighting) - now reads full-res shadow maps instead of tracing
        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pass2Pipeline);
        fixed (DescriptorSet* pDescSet2 = &pass2DescriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pass2Layout, 0, 1, pDescSet2, 0, null);
        }

        vk.CmdDispatch(commandBuffer, groupCountX, groupCountY, 1);

        InsertMemoryBarrier(commandBuffer);

        // pass2B GI downsample: dispatch at downsampled GI size
        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pass2BPipeline);
        fixed (DescriptorSet* pDescSet2B = &pass2BDescriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pass2BLayout, 0, 1, pDescSet2B, 0, null);
        }

        vk.CmdDispatch(commandBuffer, Math.Max(1, giGroupCountX), Math.Max(1, giGroupCountY), 1);

        InsertMemoryBarrier(commandBuffer);

        // pass2B GI upscale: writes full-res indirectColor -> use full-res groups
        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pass2BUpPipeline);
        fixed (DescriptorSet* pDescSet2BUp = &pass2BUpDescriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pass2BUpLayout, 0, 1, pDescSet2BUp, 0, null);
        }

        vk.CmdDispatch(commandBuffer, groupCountX, groupCountY, 1);

        InsertMemoryBarrier(commandBuffer);

        // pass3
        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pass3Pipeline);
        fixed (DescriptorSet* pDescSet3 = &pass3DescriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pass3Layout, 0, 1, pDescSet3, 0, null);
        }

        vk.CmdDispatch(commandBuffer, groupCountX, groupCountY, 1);

        InsertMemoryBarrier(commandBuffer);

        // pass4
        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pass4Pipeline);
        fixed (DescriptorSet* pDescSet4 = &pass4DescriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pass4Layout, 0, 1, pDescSet4, 0, null);
        }

        vk.CmdDispatch(commandBuffer, groupCountX, groupCountY, 1);
    }

    private void InsertMemoryBarrier(CommandBuffer commandBuffer)
    {
        MemoryBarrier barrier = new()
        {
            SType = StructureType.MemoryBarrier,
            SrcAccessMask = AccessFlags.ShaderWriteBit,
            DstAccessMask = AccessFlags.ShaderReadBit
        };

        vk.CmdPipelineBarrier(
            commandBuffer,
            PipelineStageFlags.ComputeShaderBit,
            PipelineStageFlags.ComputeShaderBit,
            0, 1, &barrier, 0, null, 0, null);
    }

    public void DestroyGBufferImages()
    {
        imageTask.DestroyImage(gPositionImage, gPositionView, gPositionMemory);
        imageTask.DestroyImage(gNormalImage, gNormalView, gNormalMemory);
        imageTask.DestroyImage(gAlbedoImage, gAlbedoView, gAlbedoMemory);
        imageTask.DestroyImage(gRayDirImage, gRayDirView, gRayDirMemory);
        imageTask.DestroyImage(litColorImage, litColorView, litColorMemory);
        imageTask.DestroyImage(indirectColorImage, indirectColorView, indirectColorMemory);
        imageTask.DestroyImage(downsampledIndirectImage, downsampledIndirectView, downsampledIndirectMemory);
        imageTask.DestroyImage(downsampledShadowImage0, downsampledShadowView0, downsampledShadowMemory0);
        imageTask.DestroyImage(downsampledShadowImage1, downsampledShadowView1, downsampledShadowMemory1);
        imageTask.DestroyImage(denoisedShadowImage0, denoisedShadowView0, denoisedShadowMemory0);
        imageTask.DestroyImage(denoisedShadowImage1, denoisedShadowView1, denoisedShadowMemory1);
        imageTask.DestroyImage(shadowFullResImage0, shadowFullResView0, shadowFullResMemory0);
        imageTask.DestroyImage(shadowFullResImage1, shadowFullResView1, shadowFullResMemory1);
        imageTask.DestroyImage(reflectedColorImage, reflectedColorView, reflectedColorMemory);
    }

    public void DestroyPipelines()
    {
        pipelineTask.DestroyPipeline(pass1Pipeline, pass1Layout, pass1Shader, pass1DescriptorLayout);
        pipelineTask.DestroyPipeline(pass2BShadowPipeline, pass2BShadowLayout, pass2BShadowShader, pass2BShadowDescriptorLayout);
        pipelineTask.DestroyPipeline(pass2BShadowDenoisePipeline, pass2BShadowDenoiseLayout, pass2BShadowDenoiseShader, pass2BShadowDenoiseDescriptorLayout);
        pipelineTask.DestroyPipeline(pass2BShadowUpPipeline, pass2BShadowUpLayout, pass2BShadowUpShader, pass2BShadowUpDescriptorLayout);
        pipelineTask.DestroyPipeline(pass2Pipeline, pass2Layout, pass2Shader, pass2DescriptorLayout);
        pipelineTask.DestroyPipeline(pass2BPipeline, pass2BLayout, pass2BShader, pass2BDescriptorLayout);
        pipelineTask.DestroyPipeline(pass2BUpPipeline, pass2BUpLayout, pass2BUpShader, pass2BUpDescriptorLayout);
        pipelineTask.DestroyPipeline(pass3Pipeline, pass3Layout, pass3Shader, pass3DescriptorLayout);
        pipelineTask.DestroyPipeline(pass4Pipeline, pass4Layout, pass4Shader, pass4DescriptorLayout);
    }

    public void DestroyDescriptorPool()
    {
        vk.DestroyDescriptorPool(device, descriptorPool, null);
    }

    private byte[] LoadShaderCode(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Shader file not found: {path}");

        return File.ReadAllBytes(path);
    }
}