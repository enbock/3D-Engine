using Infrastructure.Rendering.Vulkan.Data;
using Infrastructure.Rendering.Vulkan.Tasks;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Infrastructure.Rendering.Vulkan.Helpers;

public static unsafe class VulkanDescriptorHelper
{
    public static void CreateDescriptorSetLayoutForRaytracing(
        VulkanPipelineTask pipelineTask,
        out DescriptorSetLayout descriptorSetLayout)
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
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 2,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.ComputeBit
            },
            new()
            {
                Binding = 3,
                DescriptorType = DescriptorType.StorageBuffer,
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
            }
        ];

        descriptorSetLayout = pipelineTask.CreateDescriptorSetLayout(bindings);
    }

    public static void CreateDescriptorPoolAndSet(
        VulkanPipelineTask pipelineTask,
        DescriptorSetLayout descriptorSetLayout,
        out DescriptorPool descriptorPool,
        out DescriptorSet descriptorSet)
    {
        DescriptorPoolSize[] poolSizes =
        [
            new()
            {
                Type = DescriptorType.StorageImage,
                DescriptorCount = 1
            },
            new()
            {
                Type = DescriptorType.UniformBuffer,
                DescriptorCount = 2
            },
            new()
            {
                Type = DescriptorType.StorageBuffer,
                DescriptorCount = 3
            }
        ];

        descriptorPool = pipelineTask.CreateDescriptorPool(poolSizes, 1);
        descriptorSet = pipelineTask.AllocateDescriptorSet(descriptorPool, descriptorSetLayout);
    }

    public static void UpdateDescriptorSets(
        VulkanPipelineTask pipelineTask,
        DescriptorSet descriptorSet,
        ImageView storageImageView,
        Buffer cameraBuffer,
        Buffer triangleBuffer,
        Buffer lightBuffer,
        Buffer settingsBuffer,
        Buffer bvhBuffer)
    {
        DescriptorImageInfo imageInfo = new()
        {
            ImageLayout = ImageLayout.General,
            ImageView = storageImageView
        };

        DescriptorBufferInfo cameraBufferInfo = new()
        {
            Buffer = cameraBuffer,
            Offset = 0,
            Range = (ulong)sizeof(CameraUniformData)
        };

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
            Range = (ulong)sizeof(LightUniformData)
        };

        DescriptorBufferInfo settingsBufferInfo = new()
        {
            Buffer = settingsBuffer,
            Offset = 0,
            Range = (ulong)sizeof(RenderSettingsData)
        };

        DescriptorBufferInfo bvhBufferInfo = new()
        {
            Buffer = bvhBuffer,
            Offset = 0,
            Range = Vk.WholeSize
        };

        WriteDescriptorSet[] writes =
        [
            new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptorSet,
                DstBinding = 0,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.StorageImage,
                DescriptorCount = 1,
                PImageInfo = &imageInfo
            },
            new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptorSet,
                DstBinding = 1,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                PBufferInfo = &cameraBufferInfo
            },
            new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptorSet,
                DstBinding = 2,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                PBufferInfo = &triangleBufferInfo
            },
            new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptorSet,
                DstBinding = 3,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                PBufferInfo = &lightBufferInfo
            },
            new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptorSet,
                DstBinding = 4,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                PBufferInfo = &settingsBufferInfo
            },
            new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptorSet,
                DstBinding = 5,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.StorageBuffer,
                DescriptorCount = 1,
                PBufferInfo = &bvhBufferInfo
            }
        ];

        pipelineTask.UpdateDescriptorSets(writes);
    }
}