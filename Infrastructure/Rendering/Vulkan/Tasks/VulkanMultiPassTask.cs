using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Infrastructure.Vulkan.Tasks;

public unsafe class VulkanMultiPassTask(
    Vk vk,
    Device device,
    VulkanImageTask imageTask,
    VulkanPipelineTask pipelineTask)
{
    private DescriptorPool descriptorPool;

    private Image gAlbedoImage;
    private DeviceMemory gAlbedoMemory;
    private ImageView gAlbedoView;

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

    public void CreateGBufferImages(uint width, uint height)
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
        CreatePass2Pipeline();
        CreatePass2BPipeline();
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

        byte[] shaderCode = LoadShaderCode("Infrastructure/Rendering/Vulkan/Shaders/pass1_primary.comp.spv");
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

        pass2DescriptorLayout = pipelineTask.CreateDescriptorSetLayout(bindings);

        byte[] shaderCode = LoadShaderCode("Infrastructure/Rendering/Vulkan/Shaders/pass2_lighting.comp.spv");
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

        byte[] shaderCode = LoadShaderCode("Infrastructure/Rendering/Vulkan/Shaders/pass2b_indirect.comp.spv");
        pass2BShader = pipelineTask.CreateShaderModule(shaderCode);

        pass2BLayout = pipelineTask.CreatePipelineLayout(pass2BDescriptorLayout);
        pass2BPipeline = pipelineTask.CreateComputePipeline(pass2BShader, pass2BLayout);
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

        byte[] shaderCode = LoadShaderCode("Infrastructure/Rendering/Vulkan/Shaders/pass3_reflections.comp.spv");
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

        byte[] shaderCode = LoadShaderCode("Infrastructure/Rendering/Vulkan/Shaders/pass4_composite.comp.spv");
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
                DescriptorCount = 30
            },
            new()
            {
                Type = DescriptorType.StorageBuffer,
                DescriptorCount = 20
            },
            new()
            {
                Type = DescriptorType.UniformBuffer,
                DescriptorCount = 15
            }
        ];

        fixed (DescriptorPoolSize* pPoolSizes = poolSizes)
        {
            DescriptorPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = (uint)poolSizes.Length,
                PPoolSizes = pPoolSizes,
                MaxSets = 5
            };

            if (vk.CreateDescriptorPool(device, &poolInfo, null, out descriptorPool) != Result.Success)
                throw new Exception("Failed to create descriptor pool");
        }

        AllocateDescriptorSet(pass1DescriptorLayout, out pass1DescriptorSet);
        AllocateDescriptorSet(pass2DescriptorLayout, out pass2DescriptorSet);
        AllocateDescriptorSet(pass2BDescriptorLayout, out pass2BDescriptorSet);
        AllocateDescriptorSet(pass3DescriptorLayout, out pass3DescriptorSet);
        AllocateDescriptorSet(pass4DescriptorLayout, out pass4DescriptorSet);

        UpdatePass1DescriptorSet(cameraBuffer, triangleBuffer, bvhBuffer);
        UpdatePass2DescriptorSet(cameraBuffer, triangleBuffer, lightBuffer, settingsBuffer, bvhBuffer);
        UpdatePass2BDescriptorSet(cameraBuffer, triangleBuffer, lightBuffer, settingsBuffer, bvhBuffer);
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

        fixed (DescriptorImageInfo* pImageInfos = imageInfos)
        {
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
        }

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

        WriteDescriptorSet[] writes = new WriteDescriptorSet[10];

        DescriptorBufferInfo* pLightInfo = &lightBufferInfo;
        DescriptorBufferInfo* pTriangleInfo = &triangleBufferInfo;
        DescriptorBufferInfo* pSettingsInfo = &settingsBufferInfo;
        DescriptorBufferInfo* pCameraInfo = &cameraBufferInfo;
        DescriptorBufferInfo* pBvhInfo = &bvhBufferInfo;

        fixed (DescriptorImageInfo* pImageInfos = imageInfos)
        {
            for (uint i = 0; i < 5; i++)
                writes[i] = new WriteDescriptorSet
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = pass2DescriptorSet,
                    DstBinding = i,
                    DescriptorType = DescriptorType.StorageImage,
                    DescriptorCount = 1,
                    PImageInfo = &pImageInfos[i]
                };

            writes[5] = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = pass2DescriptorSet,
                DstBinding = 5,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                PBufferInfo = pLightInfo
            };

            writes[6] = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = pass2DescriptorSet,
                DstBinding = 6,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                PBufferInfo = pTriangleInfo
            };

            writes[7] = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = pass2DescriptorSet,
                DstBinding = 7,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                PBufferInfo = pSettingsInfo
            };

            writes[8] = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = pass2DescriptorSet,
                DstBinding = 8,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                PBufferInfo = pCameraInfo
            };

            writes[9] = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = pass2DescriptorSet,
                DstBinding = 9,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                PBufferInfo = pBvhInfo
            };
        }

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
                ImageView = indirectColorView,
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

        fixed (DescriptorImageInfo* pImageInfos = imageInfos)
        {
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
        }

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

        fixed (DescriptorImageInfo* pImageInfos = imageInfos)
        {
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
        }

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

        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pass1Pipeline);
        fixed (DescriptorSet* pDescSet1 = &pass1DescriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pass1Layout, 0, 1, pDescSet1, 0, null);
        }

        vk.CmdDispatch(commandBuffer, groupCountX, groupCountY, 1);

        InsertMemoryBarrier(commandBuffer);

        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pass2Pipeline);
        fixed (DescriptorSet* pDescSet2 = &pass2DescriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pass2Layout, 0, 1, pDescSet2, 0, null);
        }

        vk.CmdDispatch(commandBuffer, groupCountX, groupCountY, 1);

        InsertMemoryBarrier(commandBuffer);

        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pass2BPipeline);
        fixed (DescriptorSet* pDescSet2B = &pass2BDescriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pass2BLayout, 0, 1, pDescSet2B, 0, null);
        }

        vk.CmdDispatch(commandBuffer, groupCountX, groupCountY, 1);

        InsertMemoryBarrier(commandBuffer);

        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, pass3Pipeline);
        fixed (DescriptorSet* pDescSet3 = &pass3DescriptorSet)
        {
            vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pass3Layout, 0, 1, pDescSet3, 0, null);
        }

        vk.CmdDispatch(commandBuffer, groupCountX, groupCountY, 1);

        InsertMemoryBarrier(commandBuffer);

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
        imageTask.DestroyImage(reflectedColorImage, reflectedColorView, reflectedColorMemory);
    }

    public void DestroyPipelines()
    {
        pipelineTask.DestroyPipeline(pass1Pipeline, pass1Layout, pass1Shader, pass1DescriptorLayout);
        pipelineTask.DestroyPipeline(pass2Pipeline, pass2Layout, pass2Shader, pass2DescriptorLayout);
        pipelineTask.DestroyPipeline(pass2BPipeline, pass2BLayout, pass2BShader, pass2BDescriptorLayout);
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